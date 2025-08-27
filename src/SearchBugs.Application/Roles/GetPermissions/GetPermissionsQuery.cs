using MediatR;

namespace SearchBugs.Application.Roles.GetPermissions;

public sealed record GetPermissionsQuery : IRequest<List<GetPermissionsResponse>>;
