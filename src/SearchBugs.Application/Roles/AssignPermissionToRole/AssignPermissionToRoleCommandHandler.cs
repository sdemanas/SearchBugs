using MediatR;
using SearchBugs.Domain.Roles;
using Shared.Results;

namespace SearchBugs.Application.Roles.AssignPermissionToRole;

internal sealed class AssignPermissionToRoleCommandHandler
    : IRequestHandler<AssignPermissionToRoleCommand, Result>
{
    private readonly IRoleRepository _roleRepository;

    public AssignPermissionToRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        return await _roleRepository.AssignPermissionToRoleAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);
    }
}
