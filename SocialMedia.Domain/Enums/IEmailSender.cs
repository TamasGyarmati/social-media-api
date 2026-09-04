namespace SocialMedia.Domain.Enums;

public interface IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage, CancellationToken ct = default);
}