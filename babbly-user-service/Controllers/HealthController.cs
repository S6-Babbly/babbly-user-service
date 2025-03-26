using Microsoft.AspNetCore.Mvc;
using babbly_user_service.Data;

namespace babbly_user_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy" });
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
    }
} 