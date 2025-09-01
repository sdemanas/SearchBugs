namespace SearchBugs.Application.Users.GetUserRoles;

public sealed record UserRolesResponse(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<RoleDto> Roles
);

public sealed record RoleDto(
    int Id,
    string Name,
    IReadOnlyList<PermissionDto> Permissions
);

public sealed record PermissionDto(
    int Id,
    string Name,
    string Description
);
