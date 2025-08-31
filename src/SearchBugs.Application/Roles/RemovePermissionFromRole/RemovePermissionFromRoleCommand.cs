using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Roles.RemovePermissionFromRole;

public sealed record RemovePermissionFromRoleCommand(
    int RoleId,
    int PermissionId
) : IRequest<Result>;
