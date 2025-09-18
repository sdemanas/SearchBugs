using MediatR;
using SearchBugs.Domain.Roles;
using Shared.Results;

namespace SearchBugs.Application.Roles.GetRoles;

internal sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<List<GetRolesResponse>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<List<GetRolesResponse>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        // Get all roles from database
        var rolesResult = await _roleRepository.GetAllAsync(cancellationToken);
        if (rolesResult.IsFailure)
        {
            return Result.Failure<List<GetRolesResponse>>(rolesResult.Error);
        }

        var response = new List<GetRolesResponse>();

        // For each role, get its permissions
        foreach (var role in rolesResult.Value)
        {
            var permissionsResult = await _roleRepository.GetRolePermissionsAsync(role.Id, cancellationToken);

            var permissionNames = permissionsResult.IsSuccess
                ? permissionsResult.Value.Select(p => p.Name).ToArray()
                : Array.Empty<string>();

            response.Add(new GetRolesResponse(
                role.Id,
                role.Name,
                permissionNames));
        }

        return Result.Success(response);
    }
}
