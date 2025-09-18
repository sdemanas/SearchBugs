using MediatR;
using SearchBugs.Domain;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure(UserValidationErrors.UserNotFound);
        }

        var user = userResult.Value;

        // Verify current password
        if (!_passwordHashingService.VerifyPassword(request.CurrentPassword, user.Password))
        {
            return Result.Failure(UserValidationErrors.InvalidCurrentPassword);
        }

        // Hash new password
        var newHashedPassword = _passwordHashingService.HashPassword(request.NewPassword);

        user.ChangePassword(newHashedPassword);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
