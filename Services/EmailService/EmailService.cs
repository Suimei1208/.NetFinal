using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NetTechnology_Final.Models;
using Org.BouncyCastle.Utilities;

namespace NetTechnology_Final.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(EmailDto request, Accounts accounts)
        {
            var email = new MimeMessage();
            // email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailSettings:SmtpUsername").Value));
            email.From.Add(new MailboxAddress(_config.GetSection("EmailSettings:SenderName").Value, _config.GetSection("EmailSettings:SenderEmail").Value));
            email.To.Add(MailboxAddress.Parse(accounts.Email));
            email.Subject = request.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = request.SetBody("Test thử chơi chơi") };

            using var stmp = new SmtpClient();
            stmp.Connect(_config.GetSection("EmailSettings:SmtpServer").Value,
                         int.Parse(_config.GetSection("EmailSettings:SmtpPort").Value),
                         MailKit.Security.SecureSocketOptions.StartTls);
            stmp.Authenticate(_config.GetSection("EmailSettings:SmtpUsername").Value,
                _config.GetSection("EmailSettings:SmtpPassword").Value);
            stmp.Send(email);
            stmp.Disconnect(true);
        }
    }
}
