using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Roles.GetPermissions;

public sealed record GetPermissionsQuery : IRequest<Result<List<GetPermissionsResponse>>>;
