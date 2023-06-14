using Microsoft.AspNetCore.Mvc;

namespace TeamsHackathonTest.Controllers
{
    [ApiController]
    [Route("api/[Controller]/")]
    public class ChatNotificationController : Controller
    {
        [HttpGet("notification")]
        public async Task<IActionResult> GetNotification()
        {
            return Ok();
        }
    }
}
