using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Asn1.Pkcs;
using SE.Common.DTO;

namespace SE.Service.Services
{

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailData mailData);

    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _mailSettings;

        public EmailService(EmailSettings mailSettings)
        {
            _mailSettings = mailSettings;
        }

        public async Task<bool> SendEmailAsync(EmailData mailData)
        {
            try
            {
                using (var emailMessage = new MimeMessage())
                {
                    MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
                    emailMessage.From.Add(emailFrom);
                    MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToId);
                    emailMessage.To.Add(emailTo);
                    emailMessage.Subject = mailData.EmailSubject;
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = mailData.EmailBody;
                    emailMessage.Body = bodyBuilder.ToMessageBody();
                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.Connect(_mailSettings.Server, 587, MailKit.Security.SecureSocketOptions.StartTls);
                        client.Authenticate(_mailSettings.UserName, _mailSettings.Password);
                        client.Send(emailMessage);
                        client.Disconnect(true);
                    }
                };

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's requirement
                throw new Exception(ex.Message, ex);
                return false;
            }
        }
    }
}
