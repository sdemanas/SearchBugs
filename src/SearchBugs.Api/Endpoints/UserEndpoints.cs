using MediatR;
using SearchBugs.Application.Users.AssignRole;
using SearchBugs.Application.Users.ChangePassword;
using SearchBugs.Application.Users.CreateUser;
using SearchBugs.Application.Users.DeleteUser;
using SearchBugs.Application.Users.GetUserDetail;
using SearchBugs.Application.Users.GetUsers;
using SearchBugs.Application.Users.RemoveRole;
using SearchBugs.Application.Users.UpdateUser;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class UserEndpoints
{
    public record CreateUserRequest(string FirstName, string LastName, string Email, string Password, string[]? Roles = null);
    public record UpdateUserRequest(string FirstName, string LastName);
    public record AssignRoleRequest(Guid UserId, string Role);
    public record RemoveRoleRequest(Guid UserId, string Role);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("api/users")
            .WithTags("Users");

        users.MapPost("", CreateUser).WithName(nameof(CreateUser));
        users.MapGet("", GetUsers).WithName(nameof(GetUsers));
        users.MapGet("{id}", GetUserDetail).WithName(nameof(GetUserDetail));
        users.MapPut("{id}", UpdateUser).WithName(nameof(UpdateUser));
        users.MapDelete("{id}", DeleteUser).WithName(nameof(DeleteUser));
        users.MapPost("{id}/assign-role", AssignRole).WithName(nameof(AssignRole));
        users.MapPost("{id}/remove-role", RemoveRole).WithName(nameof(RemoveRole));
        users.MapPost("{id}/change-password", ChangePassword).WithName(nameof(ChangePassword));
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
        return Results.NoContent();
    }

    public static async Task<IResult> RemoveRole(
        Guid id,
        RemoveRoleRequest request,
        ISender sender)
    {
        var command = new RemoveRoleCommand(request.UserId, request.Role);
        var result = await sender.Send(command);
        return Results.Ok();
    }

    public static async Task<IResult> ChangePassword(
        Guid id,
        ChangePasswordRequest request,
        ISender sender)
    {
        var command = new ChangePasswordCommand(id, request.CurrentPassword, request.NewPassword);
        var result = await sender.Send(command);
        return Results.Ok();
    }

    public static async Task<IResult> AssignRole(
        AssignRoleRequest request,
        ISender sender)
    {
        var command = new AssignRoleCommand(request.UserId, request.Role);
        var result = await sender.Send(command);
        return Results.Ok();
    }

    public static async Task<IResult> UpdateUser(
        Guid id,
        UpdateUserRequest request,
        ISender sender)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);
        var result = await sender.Send(command);
        return Results.Ok();
    }

    public static async Task<IResult> GetUsers(ISender sender)
    {
        var query = new GetUsersQuery();
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
