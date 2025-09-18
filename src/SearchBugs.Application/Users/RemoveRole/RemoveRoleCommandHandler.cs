using MediatR;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
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
        var staticRole = Role.FromName(request.Role);
        if (staticRole is null)
        {
            return Result.Failure(UserValidationErrors.InvalidRole);
        }

        // Get the role from the database context to ensure it's tracked properly
        var roleResult = await _userRepository.GetRoleByIdAsync(staticRole.Id, cancellationToken);
        if (roleResult.IsFailure)
        {
            return Result.Failure(UserValidationErrors.InvalidRole);
        }

        user.RemoveRole(roleResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
