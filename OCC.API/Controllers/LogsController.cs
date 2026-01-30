using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public LogsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous] // For debugging purposes when auth might be broken
        public IActionResult GetLatestLog()
        {
            try
            {
                var logsPath = Path.Combine(_env.ContentRootPath, "logs");
                if (!Directory.Exists(logsPath))
                {
                    return NotFound($"Logs directory not found at {logsPath}");
                }

                var directory = new DirectoryInfo(logsPath);
                var lastLogFile = directory.GetFiles("log-*.txt")
                                           .OrderByDescending(f => f.LastWriteTime)
                                           .FirstOrDefault();

                if (lastLogFile == null)
                {
                    return NotFound("No log files found.");
                }

                // Read file with sharing enabled (in case it's being written to)
                using (var fs = new FileStream(lastLogFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    var content = sr.ReadToEnd();
                    return Ok(new 
                    { 
                        FileName = lastLogFile.Name, 
                        Timestamp = lastLogFile.LastWriteTime,
                        Content = content 
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading logs: {ex.Message}");
            }
        }
    }
}
