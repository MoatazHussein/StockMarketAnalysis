using Microsoft.AspNetCore.Mvc;
using StockMarket.Services;


namespace StockMarket.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMail([FromBody] MailRequest request)
        {
            if (request.To == null || request.Subject == null || request.Body == null)
            {
             return Ok(new { message = "one or more Email parameters Is empty" });
            }

            await _mailService.SendEmailAsync(request.To, request.Subject, request.Body);
            return Ok(new { message = "Email sent successfully" });

        }
    }

    public class MailRequest
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }

    }

}
