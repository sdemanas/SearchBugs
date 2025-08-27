using MediatR;

namespace SearchBugs.Application.Roles.GetRoles;

public sealed record GetRolesQuery : IRequest<List<GetRolesResponse>>;
