using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using System.Reflection;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Allow access without token for initialization diagnostics
    public class SystemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public SystemController(AppDbContext context, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var migrations = await _context.Database.GetAppliedMigrationsAsync();
                var connectionString = _context.Database.GetConnectionString();
                
                // Mask sensitive parts of connection string
                var maskedConnection = "Hidden";
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var parts = connectionString.Split(';');
                    var server = parts.FirstOrDefault(p => p.StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
                    var database = parts.FirstOrDefault(p => p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase) || p.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase));
                    maskedConnection = $"{server};{database}";
                }

                return Ok(new
                {
                    Environment = _env.EnvironmentName,
                    ServerTimeUtc = DateTime.UtcNow,
                    Database = maskedConnection,
                    AppliedMigrations = migrations,
                    AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    Status = "Running"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Trace = ex.StackTrace });
            }
        }
    }
}
