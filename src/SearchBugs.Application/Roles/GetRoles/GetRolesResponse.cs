namespace SearchBugs.Application.Roles.GetRoles;

public record GetRolesResponse(
    int Id,
    string Name,
    string[] Permissions);
