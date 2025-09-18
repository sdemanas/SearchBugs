using MediatR;
using SearchBugs.Domain.Roles;
using Shared.Results;

namespace SearchBugs.Application.Roles.GetRoleWithPermissions;

internal sealed class GetRoleWithPermissionsQueryHandler
    : IRequestHandler<GetRoleWithPermissionsQuery, Result<GetRoleWithPermissionsResponse>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleWithPermissionsQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<GetRoleWithPermissionsResponse>> Handle(GetRoleWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Get role by ID
        var roleResult = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (roleResult.IsFailure)
        {
            return Result.Failure<GetRoleWithPermissionsResponse>(roleResult.Error);
        }

        var role = roleResult.Value;

        // Get permissions for the role
        var permissionsResult = await _roleRepository.GetRolePermissionsAsync(request.RoleId, cancellationToken);
        if (permissionsResult.IsFailure)
        {
            return Result.Failure<GetRoleWithPermissionsResponse>(permissionsResult.Error);
        }

        var permissions = permissionsResult.Value
            .Select(p => new PermissionDto(p.Id, p.Name, p.Description))
            .ToArray();

        var response = new GetRoleWithPermissionsResponse(
            role.Id,
            role.Name,
            permissions);

        return Result.Success(response);
    }
}
