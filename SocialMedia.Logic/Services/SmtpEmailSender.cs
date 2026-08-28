using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace SocialMedia.Logic.Services;

public class SmtpEmailSender(IConfiguration config) : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(config["Smtp:Name"] ?? "", config["Smtp:Address"] ?? ""));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        var host = config["Smtp:Host"] ?? "localhost";
        var port = int.Parse(config["Smtp:Port"] ?? "1025");
        
        await client.ConnectAsync(host, port, SecureSocketOptions.None);
        
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}