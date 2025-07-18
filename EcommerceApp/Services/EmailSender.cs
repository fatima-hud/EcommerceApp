using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;


namespace EcommerceApp.Services
{
    public class EmailSender : IEmailSender
    { 
        private readonly EmailSettings _emailSettings;
  public EmailSender(IOptions<EmailSettings>options)
        {
            _emailSettings = options.Value;
        }

        public async Task SendEmailAsync(string Email, string subject, string htmlMessage)
        {
            try
            {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.Email);
             email.From.Add(new MailboxAddress("Ecommerce App", _emailSettings.Email));
                email.To.Add(MailboxAddress.Parse(Email));
            email.Subject = subject;
           
           
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage // Set HTML content
            };
            email.Body = builder.ToMessageBody();
            // Create an SmtpClient using MailKit and connect to SMTP server
           
                using (var smtp = new SmtpClient())
                {
                    // Connect to the SMTP server using the settings
                   await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.SslOnConnect);
                  //  await smtp.ConnectAsync(_emailSettings.Host,_emailSettings.Port,SecureSocketOptions.StartTls );
 
                    // Authenticate using the email credentials
                    await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
                    smtp.Timeout = 10000; // تعيين وقت الانتظار بالمللي ثانية (10 ثوانٍ)

                    // Send the email
                    await smtp.SendAsync(email);

                   // Disconnect after sending the email
                   await smtp.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
