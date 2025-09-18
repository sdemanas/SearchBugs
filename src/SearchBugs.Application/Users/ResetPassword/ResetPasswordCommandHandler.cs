using MediatR;
using Microsoft.Extensions.Logging;
using SearchBugs.Domain;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.Users.ResetPassword;

internal sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Create email value object
        var email = Email.Create(request.Email);

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
            return Result.Failure(UserValidationErrors.InvalidPasswordResetToken);
        }

        // Validate reset token
        if (!user.IsPasswordResetTokenValid(request.Token))
        {
            _logger.LogWarning("Invalid password reset token used for user: {Email}", request.Email);
            return Result.Failure(UserValidationErrors.InvalidPasswordResetToken);
        }

        // Hash new password
        var hashedPassword = _passwordHashingService.HashPassword(request.NewPassword);

        // Update password and clear reset token
        user.ChangePassword(hashedPassword);
        user.ClearPasswordResetToken();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password successfully reset for user: {Email}", request.Email);

        return Result.Success();
    }
}
