using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController(IEmailSender emailSender) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> SendEmail()
        {
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted, timeoutCts.Token);
            await emailSender.SendEmailAsync("test@example.com", "Test Subject", "Test Message", linkedCts.Token);

            return Ok("Email sent successfully.");
        }
        
    }
}
