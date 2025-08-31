using MediatR;
using SearchBugs.Application.Roles.AssignPermissionToRole;
using SearchBugs.Application.Roles.GetPermissions;
using SearchBugs.Application.Roles.GetRoleWithPermissions;
using SearchBugs.Application.Roles.GetRoles;
using SearchBugs.Application.Roles.RemovePermissionFromRole;
using SearchBugs.Api.Extensions;
using Shared.Results;

namespace SearchBugs.Api.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var roles = app.MapGroup("api/roles")
            .WithTags("Roles");

        roles.MapGet("", GetRoles).WithName(nameof(GetRoles));
        roles.MapGet("permissions", GetPermissions).WithName(nameof(GetPermissions));
        roles.MapGet("{roleId:int}", GetRoleWithPermissions).WithName(nameof(GetRoleWithPermissions));
        roles.MapPost("{roleId:int}/permissions/{permissionId:int}", AssignPermissionToRole).WithName(nameof(AssignPermissionToRole));
        roles.MapDelete("{roleId:int}/permissions/{permissionId:int}", RemovePermissionFromRole).WithName(nameof(RemovePermissionFromRole));
    }

    public static async Task<IResult> GetRoles(ISender sender)
    {
        var query = new GetRolesQuery();
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetPermissions(ISender sender)
    {
        var query = new GetPermissionsQuery();
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetRoleWithPermissions(int roleId, ISender sender)
    {
        var query = new GetRoleWithPermissionsQuery(roleId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> AssignPermissionToRole(int roleId, int permissionId, ISender sender)
    {
        var command = new AssignPermissionToRoleCommand(roleId, permissionId);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> RemovePermissionFromRole(int roleId, int permissionId, ISender sender)
    {
        var command = new RemovePermissionFromRoleCommand(roleId, permissionId);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }
}
