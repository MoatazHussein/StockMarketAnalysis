using System.Net.Mail;
using System.Net;

namespace StockMarket.Services.Email
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            string? Host = Environment.GetEnvironmentVariable("Host")?.ToString();
            string? Port = Environment.GetEnvironmentVariable("Port");
            string? Username = Environment.GetEnvironmentVariable("Username");
            string? Password = Environment.GetEnvironmentVariable("Password");
            if (Host != null && Port != null && Username != null && Password != null)
            {
                var smtpClient = new SmtpClient(Host)
                {
                    Port = int.Parse(Port),
                    Credentials = new NetworkCredential(Username, Password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(Username),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                //smtpClient.UseDefaultCredentials = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
