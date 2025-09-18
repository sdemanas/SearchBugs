namespace SearchBugs.Domain.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);
}
