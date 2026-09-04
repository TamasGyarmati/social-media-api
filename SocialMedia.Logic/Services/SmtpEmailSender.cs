using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Logic.Services;

public class SmtpEmailSender(IConfiguration config) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage,  CancellationToken ct)
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
        
        await client.ConnectAsync(host, port, SecureSocketOptions.None, ct);
        
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}