using MediatR;
using SearchBugs.Application.Roles.GetPermissions;
using SearchBugs.Application.Roles.GetRoles;

namespace SearchBugs.Api.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var roles = app.MapGroup("api/roles")
            .WithTags("Roles");

        roles.MapGet("", GetRoles).WithName(nameof(GetRoles));
        roles.MapGet("permissions", GetPermissions).WithName(nameof(GetPermissions));
    }

    public static async Task<IResult> GetRoles(ISender sender)
    {
        var query = new GetRolesQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetPermissions(ISender sender)
    {
        var query = new GetPermissionsQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
