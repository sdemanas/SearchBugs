using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Api.Extensions;
using SearchBugs.Application.Users.AssignRole;
using SearchBugs.Application.Users.AssignRoleToUser;
using SearchBugs.Application.Users.ChangePassword;
using SearchBugs.Application.Users.CreateUser;
using SearchBugs.Application.Users.DeleteUser;
using SearchBugs.Application.Users.GetUserDetail;
using SearchBugs.Application.Users.GetUserRoles;
using SearchBugs.Application.Users.GetUsers;
using SearchBugs.Application.Users.RemoveRole;
using SearchBugs.Application.Users.RemoveRoleFromUser;
using SearchBugs.Application.Users.UpdateUser;

namespace SearchBugs.Api.Endpoints;

public static class UserEndpoints
{
    public record CreateUserRequest(string FirstName, string LastName, string Email, string Password, string[]? Roles = null);
    public record UpdateUserRequest(string FirstName, string LastName);
    public record AssignRoleRequest(Guid UserId, string Role);
    public record AssignRoleToUserRequest(Guid UserId, int RoleId);
    public record RemoveRoleRequest(Guid UserId, string Role);
    public record RemoveRoleFromUserRequest(Guid UserId, int RoleId);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("api/users")
            .WithTags("Users")
            .WithOpenApi();

        users.MapGet("/", GetUsers).WithName(nameof(GetUsers)).RequireAuthorization("ListAllUsers");
        users.MapGet("/{id:guid}", GetUserDetail).WithName(nameof(GetUserDetail)).RequireAuthorization("ViewUserDetails");
        users.MapGet("/{id:guid}/roles", GetUserRoles).WithName(nameof(GetUserRoles)).RequireAuthorization("ViewUserDetails");
        users.MapPost("/", CreateUser).WithName(nameof(CreateUser)).RequireAuthorization("CreateUser");
        users.MapPut("/{id:guid}", UpdateUser).WithName(nameof(UpdateUser)).RequireAuthorization("UpdateUser");
        users.MapDelete("/{id:guid}", DeleteUser).WithName(nameof(DeleteUser)).RequireAuthorization("DeleteUser");
        users.MapPost("/{id:guid}/roles", AssignRoleToUser).WithName(nameof(AssignRoleToUser)).RequireAuthorization("AssignRoleToUser");
        users.MapDelete("/{id:guid}/roles/{roleId:int}", RemoveRoleFromUser).WithName(nameof(RemoveRoleFromUser)).RequireAuthorization("RemoveRoleFromUser");
        users.MapPut("/{id:guid}/change-password", ChangePassword).WithName(nameof(ChangePassword)).RequireAuthorization("ChangeUserPassword");

        // Legacy endpoints (keeping for backward compatibility)
        users.MapPost("/{id:guid}/assign-role", AssignRole).WithName("AssignRoleLegacy").RequireAuthorization("AssignRoleToUser");
        users.MapDelete("/{id:guid}/remove-role", RemoveRole).WithName("RemoveRoleLegacy").RequireAuthorization("RemoveRoleFromUser");
    }

    public static async Task<IResult> GetUserRoles(
        Guid id,
        ISender sender)
    {
        var query = new GetUserRolesQuery(id);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> AssignRoleToUser(
        Guid id,
        [FromBody] AssignRoleToUserRequest request,
        ISender sender)
    {
        var command = new AssignRoleToUserCommand(id, request.RoleId);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> RemoveRoleFromUser(
        Guid id,
        int roleId,
        ISender sender)
    {
        var command = new RemoveRoleFromUserCommand(id, roleId);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> CreateUser(
        CreateUserRequest request,
        ISender sender)
    {
        var command = new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Roles);

        var result = await sender.Send(command);

        // Extract UserId for the location header
        var resultType = result!.GetType();
        var valueProperty = resultType.GetProperty("Value");
        if (valueProperty != null)
        {
            var value = valueProperty.GetValue(result);
            var userIdProperty = value?.GetType().GetProperty("UserId");
            var userId = userIdProperty?.GetValue(value)?.ToString();
            return result.ToCreatedResult($"/api/users/{userId}");
        }

        return result.ToHttpResult();
    }

    public static async Task<IResult> DeleteUser(
        Guid id,
        ISender sender)
    {
        var command = new DeleteUserCommand(id);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> RemoveRole(
        Guid id,
        [FromBody] RemoveRoleRequest request,
        ISender sender)
    {
        var command = new RemoveRoleCommand(request.UserId, request.Role);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> ChangePassword(
        Guid id,
        [FromBody] ChangePasswordRequest request,
        ISender sender)
    {
        var command = new ChangePasswordCommand(id, request.CurrentPassword, request.NewPassword);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> AssignRole(
        [FromBody] AssignRoleRequest request,
        ISender sender)
    {
        var command = new AssignRoleCommand(request.UserId, request.Role);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        ISender sender)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetUsers(
        ISender sender,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetUsersQuery(searchTerm, roleFilter, pageNumber, pageSize);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetUserDetail(
        Guid id,
        ISender sender)
    {
        var query = new GetUserDetailQuery(id);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }
}
