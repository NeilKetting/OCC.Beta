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
            var serverTime = DateTime.UtcNow;
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            var envName = _env.EnvironmentName;
            
            IEnumerable<string> migrations = new List<string>();
            string? dbError = null;
            string maskedConnection = "Hidden";

            try
            {
                var connectionString = _context.Database.GetConnectionString();
                
                // Mask sensitive parts of connection string
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var parts = connectionString.Split(';');
                    var server = parts.FirstOrDefault(p => p.StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
                    var database = parts.FirstOrDefault(p => p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase) || p.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase));
                    maskedConnection = $"{server};{database}";
                }

                migrations = await _context.Database.GetAppliedMigrationsAsync();
            }
            catch (Exception ex)
            {
                dbError = ex.Message;
            }

            return Ok(new
            {
                Environment = envName,
                ServerTimeUtc = serverTime,
                Database = maskedConnection,
                DatabaseError = dbError,
                AppliedMigrations = migrations,
                AssemblyVersion = assemblyVersion,
                Status = dbError == null ? "Running" : "Degraded (DB Connection Error)"
            });
        }
    }
}
