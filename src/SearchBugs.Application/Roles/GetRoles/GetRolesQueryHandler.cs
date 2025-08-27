using MediatR;
using SearchBugs.Domain.Roles;

namespace SearchBugs.Application.Roles.GetRoles;

internal sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<GetRolesResponse>>
{
    public Task<List<GetRolesResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = Role.GetValues().Select(role => new GetRolesResponse(
            role.Id,
            role.Name,
            role.Permissions?.Select(p => p.Name).ToArray() ?? Array.Empty<string>()
        )).ToList();

        return Task.FromResult(roles);
    }
}
