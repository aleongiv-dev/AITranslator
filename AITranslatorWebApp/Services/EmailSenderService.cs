using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace AITranslatorWebApp.Services
{
    public class EmailSenderService
    {
        private readonly ILogger<EmailSenderService> _logger;
        private readonly string _gmailAddress;
        private readonly string _gmailPassword;
        private readonly string _smtpHost;
        private readonly int _smtpPort;

        public EmailSenderService(ILogger<EmailSenderService> logger, IConfiguration config)
        {
            _logger = logger;

            // Read from appsettings.json or environment variables
            _gmailAddress = config["EmailSettings:GmailAddress"];
            _gmailPassword = config["EmailSettings:GmailPassword"];
            _smtpHost = config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.TryParse(config["EmailSettings:SmtpPort"], out var port) ? port : 587;
        }

        public async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("AI Error Support", _gmailAddress));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_gmailAddress, _gmailPassword);

                await smtp.SendAsync(email);
                _logger.LogInformation("Email successfully sent to {0}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gmail SMTP failure.");
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
