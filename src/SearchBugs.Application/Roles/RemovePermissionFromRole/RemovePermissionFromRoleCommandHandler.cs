using MediatR;
using SearchBugs.Domain.Roles;
using Shared.Results;

namespace SearchBugs.Application.Roles.RemovePermissionFromRole;

internal sealed class RemovePermissionFromRoleCommandHandler
    : IRequestHandler<RemovePermissionFromRoleCommand, Result>
{
    private readonly IRoleRepository _roleRepository;

    public RemovePermissionFromRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        return await _roleRepository.RemovePermissionFromRoleAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);
    }
}
