using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Users.AssignRole;

internal sealed class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
{

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _roleRepository = roleRepository;
    }


    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {

        var user = await _userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }
        var role = await _roleRepository.GetByNameAsync(request.Role, cancellationToken);

        if (role.IsFailure)
        {
            return Result.Failure(role.Error);
        }

        // Check if user already has this role
        if (user.Value.Roles.Any(r => r.Id == role.Value.Id))
        {
            return Result.Failure(new Error("UserRole.Conflict", "User already has this role"));
        }

        user.Value.AddRole(role.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
