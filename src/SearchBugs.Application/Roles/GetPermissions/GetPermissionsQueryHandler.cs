using MediatR;
using SearchBugs.Domain.Roles;

namespace SearchBugs.Application.Roles.GetPermissions;

internal sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<GetPermissionsResponse>>
{
    public Task<List<GetPermissionsResponse>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = Permission.GetValues().Select(permission => new GetPermissionsResponse(
            permission.Id,
            permission.Name,
            permission.Description
        )).ToList();

        return Task.FromResult(permissions);
    }
}
