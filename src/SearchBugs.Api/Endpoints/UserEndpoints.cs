﻿using MediatR;
using SearchBugs.Application.Users.AssignRole;
using SearchBugs.Application.Users.GetUserDetail;
using SearchBugs.Application.Users.GetUsers;
using SearchBugs.Application.Users.UpdateUser;

namespace SearchBugs.Api.Endpoints;

public static class UserEndpoints
{

    public record UpdateUserRequest(string FirstName, string LastName);
    public record AssignRoleRequest(Guid UserId, string Role);

    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("api/users");
        users.MapGet("", GetUsers).WithName(nameof(GetUsers));
        users.MapGet("{id}", GetUserDetail).WithName(nameof(GetUserDetail));
        users.MapPut("{id}", UpdateUser).WithName(nameof(UpdateUser));
        users.MapPost("{id}/assign-role", AssignRole).WithName(nameof(AssignRole));
    }

    public static async Task<IResult> AssignRole(
        AssignRoleRequest request,
        ISender sender)
    {
        var command = new AssignRoleCommand(request.UserId, request.Role);
        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    public static async Task<IResult> UpdateUser(
        Guid id,
        UpdateUserRequest request,
        ISender sender)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);
        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetUsers(ISender sender)
    {
        var query = new GetUsersQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetUserDetail(
        Guid id,
        ISender sender)
    {
        var query = new GetUserDetailQuery(id);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
