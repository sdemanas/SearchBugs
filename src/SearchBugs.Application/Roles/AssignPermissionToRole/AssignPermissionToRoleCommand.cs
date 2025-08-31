using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Roles.AssignPermissionToRole;

public sealed record AssignPermissionToRoleCommand(
    int RoleId,
    int PermissionId
) : IRequest<Result>;
