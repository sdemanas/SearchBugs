using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Roles.GetRoleWithPermissions;

public sealed record GetRoleWithPermissionsQuery(int RoleId) : IRequest<Result<GetRoleWithPermissionsResponse>>;
