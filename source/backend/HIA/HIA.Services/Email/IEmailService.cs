using HIA.Core.Models.Files;

namespace HIA.Services.Email;

public interface IEmailService
{
    void SendEmailNewUser(string url, string fullname, string email, bool isNewUser);
    void SendEmailAttachmentWithBytes(string toEmail, FileContentModel fileContent);
}
