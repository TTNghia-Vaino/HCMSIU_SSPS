using HCMSIU_SSPS.Models;
using System.Net;
using System.Net.Mail;

namespace HCMSIU_SSPS.Services
{
    public interface IEmailService
    {
        public Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                string smtpSettings = smtpEnum.SmtpSettings.SmtpSettings.ToString();
                string host = _configuration[$"{smtpSettings}:{smtpEnum.SmtpSettings.Host}"];
                int.TryParse(_configuration[$"{smtpSettings}:{smtpEnum.SmtpSettings.Port}"], out int port);
                string username = _configuration[$"{smtpSettings}:{smtpEnum.SmtpSettings.Username}"];
                string password = _configuration[$"{smtpSettings}:{smtpEnum.SmtpSettings.Password}"];
                var smtpClient = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage()
                {
                    From = new MailAddress(username, "HCMSIU SSPS"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception) 
            {
                throw;

            }
        }
    }
}
