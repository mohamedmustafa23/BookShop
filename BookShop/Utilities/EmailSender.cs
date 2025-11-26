using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace BookShop.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("MohamedMustafa2362001@gmail.com", "tasl ityv ihel fbmw")
            };
            return client.SendMailAsync(new MailMessage("MohamedMustafa2362001@gmail.com", email, subject, htmlMessage)
            {
                IsBodyHtml = true
            });
        }
    }
}
