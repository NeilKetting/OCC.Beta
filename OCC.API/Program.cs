using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OCC.API.Data;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt");
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 2)
    .CreateLogger();

builder.Host.UseSerilog();

// Always load appsettings.secrets.json if it exists (for local secrets or production overrides)
builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Add services to the container.
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<OCC.API.Infrastructure.Filters.ConcurrencyExceptionFilter>();
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// (Optional) Add DbInitializer if you want to use it as a service, 
// but usually we call it in the app scope below.

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Handle SignalR authentication via Query String
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// SignalR
builder.Services.AddSignalR();

// Email Service (Mock/Local for Dev)
builder.Services.AddSingleton<OCC.API.Services.IEmailService, OCC.API.Services.MockEmailService>();
// Security
builder.Services.AddScoped<OCC.API.Services.PasswordHasher>();
builder.Services.AddScoped<OCC.API.Services.IAuthService, OCC.API.Services.AuthService>();
builder.Services.AddHostedService<OCC.API.Services.DatabaseBackupService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline.
// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting Database Initialization...");
        var context = services.GetRequiredService<AppDbContext>();
        
        // Log connection string (masked)
        var conn = context.Database.GetConnectionString();
        logger.LogInformation($"Using Connection String: {conn?.Split(';')[0]}... (Length: {conn?.Length})");

        var hasher = services.GetRequiredService<OCC.API.Services.PasswordHasher>();
        
        logger.LogInformation("Calling DbInitializer.Initialize()...");
        DbInitializer.Initialize(context, hasher, app.Environment.IsDevelopment(), logger);
        logger.LogInformation("Database Initialization Completed Successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "FATAL ERROR: Parsing Database Migration failed.");
        if (ex.InnerException != null)
        {
             logger.LogCritical(ex.InnerException, "Inner Exception detected.");
        }
    }
}

// Enable Swagger in all environments for now
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseHttpMethodOverride();

app.UseAuthentication();
app.UseAuthorization();

// app.UseHttpMethodOverride(); // Removed from here

app.MapControllers();
app.MapHub<OCC.API.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
