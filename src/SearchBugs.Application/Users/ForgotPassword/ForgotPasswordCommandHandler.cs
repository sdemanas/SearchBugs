using MediatR;
using Microsoft.Extensions.Logging;
using SearchBugs.Domain;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;
using Shared.Time;
using System.Security.Cryptography;

namespace SearchBugs.Application.Users.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Create email value object
        var email = Email.Create(request.Email);

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        // Always return success to prevent email enumeration attacks
        // But only send email if user exists
        if (user != null)
        {
            // Generate secure reset token
            var resetToken = GenerateSecureToken();
            var tokenExpiry = SystemTime.UtcNow.AddHours(1); // Token expires in 1 hour

            // Set reset token on user
            user.SetPasswordResetToken(resetToken, tokenExpiry);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send reset email
            try
            {
                await _emailService.SendPasswordResetEmailAsync(
                    request.Email,
                    resetToken,
                    user.Name.FirstName,
                    cancellationToken);

                _logger.LogInformation("Password reset email sent to user with email: {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to: {Email}", request.Email);
                // Don't fail the operation if email sending fails
            }
        }
        else
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
        }

        return Result.Success();
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
