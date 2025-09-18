using MediatR;
using SearchBugs.Domain.Roles;
using Shared.Results;

namespace SearchBugs.Application.Roles.GetPermissions;

internal sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<List<GetPermissionsResponse>>>
{
    public Task<Result<List<GetPermissionsResponse>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = Permission.GetValues().Select(permission => new GetPermissionsResponse(
            permission.Id,
            permission.Name,
            permission.Description
        )).ToList();

        return Task.FromResult(Result.Success(permissions));
    }
}
