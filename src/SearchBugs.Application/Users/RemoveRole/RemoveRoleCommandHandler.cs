using MediatR;
using SearchBugs.Domain.Users;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain;
using Shared.Results;

namespace SearchBugs.Application.Users.RemoveRole;

internal sealed class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure(UserValidationErrors.UserNotFound);
        }

        var user = userResult.Value;
        var role = Role.FromName(request.Role);
        if (role is null)
        {
            return Result.Failure(UserValidationErrors.InvalidRole);
        }

        user.RemoveRole(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
