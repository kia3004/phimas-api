using Microsoft.AspNetCore.Mvc;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API is alive");
        }
    }
}