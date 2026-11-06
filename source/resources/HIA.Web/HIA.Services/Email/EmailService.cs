using HIA.Core.Base;
using HIA.Core.Models.Files;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace HIA.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;
        private MailboxAddress _from;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<AppSettings> appSettings,
            ILogger<EmailService> logger
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _from = new MailboxAddress(_appSettings?.EmailConfig?.Sender ?? "", _appSettings?.EmailConfig?.Email ?? "");
        }

        public void SendEmailNewUser(string url, string fullname, string email, bool isNewUser)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(_from);
            MailboxAddress to = new MailboxAddress(fullname, email);
            message.To.Add(to);

            message.Subject = isNewUser ? _appSettings.EmailConfig.NewUser.Subject : _appSettings.EmailConfig.ResetPass.Subject;
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = isNewUser
                ? $"<h4>Chào mừng {fullname}  đến với hệ thống Giám Định Bảo Hiểm HIA</h4>" +
                $"Vui lòng nhấn vào đường <a href='{url}'>link</a> để khởi tạo mật khẩu."
                : $"<h4>Xin chào {fullname},</h4>" +
                $"Vui lòng nhấn vào đường  <a href='{url}'>link</a> để đặt lại mật khẩu.";
            message.Body = bodyBuilder.ToMessageBody();

            _ = SendEmailAsync(message);
        }

        public void SendEmailAttachmentWithBytes(string toEmail, FileContentModel fileContent)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(_from);
            MailboxAddress to = new MailboxAddress(toEmail, toEmail);
            message.To.Add(to);

            message.Subject = "[HIA] Báo cáo hồ sơ giám định";
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $"<h4>Xin chào,</h4>" +
                $"Báo cáo chi tiết lỗi được gửi trong file đính kèm.";
            bodyBuilder.Attachments.Add(fileContent.Name, fileContent.ByteArray, new ContentType("application", fileContent.ContentType));

            message.Body = bodyBuilder.ToMessageBody();

            _ = SendEmailAsync(message);
        }
        private async Task SendEmailAsync(MimeMessage message)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Connect(_appSettings.EmailConfig.Domain, _appSettings.EmailConfig.Port, true);
                client.Authenticate(_appSettings.EmailConfig.Email, _appSettings.EmailConfig.Password);

                await client.SendAsync(message);
                client.Disconnect(true);
                client.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SendEmailAsync)}: {ex.Message}");
            }
        }
    }
}
