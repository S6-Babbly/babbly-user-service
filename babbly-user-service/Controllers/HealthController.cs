using Microsoft.AspNetCore.Mvc;
using babbly_user_service.Data;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace babbly_user_service.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", service = "babbly-user-service" });
        }

        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Simple check to see if database is accessible
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    return Ok(new { status = "Database connection healthy" });
                }
                else
                {
                    return StatusCode(500, new { status = "Database connection failed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Database connection error", message = ex.Message });
            }
        }

        [HttpGet("performance")]
        public async Task<IActionResult> PerformanceTest([FromQuery] int delayMs = 0, [FromQuery] int cpuLoadMs = 0)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Optional artificial delay to simulate network latency
            if (delayMs > 0)
            {
                await Task.Delay(Math.Min(delayMs, 10000)); // Cap at 10 seconds
            }

            // Optional CPU load simulation
            if (cpuLoadMs > 0)
            {
                // Simulate CPU load by performing calculations
                SimulateCpuLoad(Math.Min(cpuLoadMs, 5000)); // Cap at 5 seconds
            }

            // Check database response time
            var dbStopwatch = new Stopwatch();
            dbStopwatch.Start();
            var canConnect = await _context.Database.CanConnectAsync();
            dbStopwatch.Stop();

            stopwatch.Stop();

            // Get system metrics
            var systemInfo = new
            {
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet / 1024 / 1024, // Memory in MB
                ProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds
            };

            return Ok(new
            {
                Timestamp = DateTime.UtcNow,
                TotalResponseTimeMs = stopwatch.ElapsedMilliseconds,
                DatabaseResponseTimeMs = dbStopwatch.ElapsedMilliseconds,
                DatabaseConnected = canConnect,
                SystemInfo = systemInfo
            });
        }

        private void SimulateCpuLoad(int durationMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < durationMs)
            {
                // Perform CPU-intensive calculations
                for (int i = 0; i < 1000000; i++)
                {
                    Math.Sqrt(i * Math.PI);
                }
            }
        }
    }
} 