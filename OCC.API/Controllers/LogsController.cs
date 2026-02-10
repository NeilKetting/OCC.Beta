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
        [AllowAnonymous]
        public IActionResult GetLatestLog([FromQuery] int lines = 1000)
        {
            try
            {
                var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
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

                // Read file with sharing enabled
                using (var fs = new FileStream(lastLogFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    if (lines > 0)
                    {
                        // Efficient reverse reading would be better, but for now just read all and take last N
                        // Beware of memory for huge files, but safe enough for text logs
                        var allLines = new List<string>();
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            allLines.Add(line);
                        }
                        
                        var content = string.Join("\n", allLines.TakeLast(lines));
                        return Ok(new 
                        { 
                            FileName = lastLogFile.Name, 
                            Timestamp = lastLogFile.LastWriteTime,
                            Content = content 
                        });
                    }
                    else
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
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading logs: {ex.Message}");
            }
        }
    }
}
