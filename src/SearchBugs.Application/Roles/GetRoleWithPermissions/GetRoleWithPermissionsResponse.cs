namespace SearchBugs.Application.Roles.GetRoleWithPermissions;

public record GetRoleWithPermissionsResponse(
    int Id,
    string Name,
    PermissionDto[] Permissions);

public record PermissionDto(
    int Id,
    string Name,
    string Description);
