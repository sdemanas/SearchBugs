using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Roles.GetRoles;

public sealed record GetRolesQuery : IRequest<Result<List<GetRolesResponse>>>;
