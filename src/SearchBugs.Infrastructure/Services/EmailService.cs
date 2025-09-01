using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SearchBugs.Domain.Services;
using System.Net;
using System.Net.Mail;

namespace SearchBugs.Infrastructure.Services;

internal sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailConfig = _configuration.GetSection("EmailSettings");
            var fromEmail = emailConfig["FromEmail"] ?? "noreply@searchbugs.com";
            var smtpHost = emailConfig["SmtpHost"] ?? "localhost";
            var smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
            var smtpUser = emailConfig["SmtpUser"] ?? "";
            var smtpPassword = emailConfig["SmtpPassword"] ?? "";
            var enableSsl = bool.Parse(emailConfig["EnableSsl"] ?? "true");

            var subject = "Password Reset Request";
            var resetUrl = $"{_configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173"}/reset-password?token={resetToken}&email={toEmail}";

            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {userName},</p>
                    <p>You requested a password reset for your SearchBugs account.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href=""{resetUrl}"" style=""background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Reset Password</a></p>
                    <p>If you didn't request this password reset, please ignore this email.</p>
                    <p>This link will expire in 1 hour.</p>
                    <br>
                    <p>Best regards,<br>SearchBugs Team</p>
                </body>
                </html>";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);

            // For development/testing, we'll log the token instead of failing
            if (_configuration.GetValue<bool>("Development", false))
            {
                _logger.LogWarning("Development mode: Password reset token for {Email}: {Token}", toEmail, resetToken);
            }
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailConfig = _configuration.GetSection("EmailSettings");
            var fromEmail = emailConfig["FromEmail"] ?? "noreply@searchbugs.com";
            var smtpHost = emailConfig["SmtpHost"] ?? "localhost";
            var smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
            var smtpUser = emailConfig["SmtpUser"] ?? "";
            var smtpPassword = emailConfig["SmtpPassword"] ?? "";
            var enableSsl = bool.Parse(emailConfig["EnableSsl"] ?? "true");

            var subject = "Welcome to SearchBugs!";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to SearchBugs!</h2>
                    <p>Hello {userName},</p>
                    <p>Welcome to SearchBugs! Your account has been successfully created.</p>
                    <p>You can now log in and start tracking bugs and issues.</p>
                    <br>
                    <p>Best regards,<br>SearchBugs Team</p>
                </body>
                </html>";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
        }
    }
}
