using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Application.Notifications;
using SearchBugs.Application.Notifications.GetUserNotifications;
using SearchBugs.Application.Notifications.GetUnreadNotifications;
using SearchBugs.Application.Notifications.GetUnreadCount;
using SearchBugs.Application.Notifications.MarkAsRead;
using SearchBugs.Application.Notifications.MarkAllAsRead;
using SearchBugs.Application.Notifications.DeleteNotification;
using SearchBugs.Application.Notifications.ClearAllNotifications;
using SearchBugs.Application.Notifications.SendNotification;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        // Retrieval endpoints
        group.MapGet("/", GetUserNotifications).RequireAuthorization();
        group.MapGet("/unread", GetUnreadNotifications).RequireAuthorization();
        group.MapGet("/unread-count", GetUnreadCount).RequireAuthorization();
        group.MapPut("/{id}/read", MarkAsRead).RequireAuthorization();
        group.MapPut("/mark-all-read", MarkAllAsRead).RequireAuthorization();
        group.MapDelete("/{id}", DeleteNotification).RequireAuthorization();
        group.MapDelete("/clear-all", ClearAllNotifications).RequireAuthorization();

        // Sending endpoints
        group.MapPost("/send", SendNotification).RequireAuthorization("ViewNotification");
        group.MapPost("/send-to-user", SendNotificationToUser).RequireAuthorization("ViewNotification");
        group.MapPost("/broadcast", BroadcastNotification).RequireAuthorization("ViewNotification");
        group.MapPost("/bug-notification", SendBugNotification).RequireAuthorization("ViewNotification");
    }

    // Retrieval methods
    private static async Task<IResult> GetUserNotifications(
        [FromServices] ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null)
    {
        var query = new GetUserNotificationsQuery(pageNumber, pageSize, isRead);
        var result = await sender.Send(query);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> GetUnreadNotifications(
        [FromServices] ISender sender)
    {
        var query = new GetUnreadNotificationsQuery();
        var result = await sender.Send(query);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> GetUnreadCount(
        [FromServices] ISender sender)
    {
        var query = new GetUnreadCountQuery();
        var result = await sender.Send(query);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> MarkAsRead(
        [FromRoute] string id,
        [FromServices] ISender sender)
    {
        var command = new MarkAsReadCommand(id);
        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> MarkAllAsRead(
        [FromServices] ISender sender)
    {
        var command = new MarkAllAsReadCommand();
        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> DeleteNotification(
        [FromRoute] string id,
        [FromServices] ISender sender)
    {
        var command = new DeleteNotificationCommand(id);
        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> ClearAllNotifications(
        [FromServices] ISender sender)
    {
        var command = new ClearAllNotificationsCommand();
        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> SendNotification(
        [FromBody] SendNotificationRequest request,
        [FromServices] ISender sender)
    {
        var command = new SendNotificationCommand(
            request.UserId,
            request.Type,
            request.Message,
            request.Data);

        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> SendNotificationToUser(
        [FromBody] SendNotificationToUserRequest request,
        [FromServices] ISender sender)
    {
        var command = new SendNotificationCommand(
            request.UserId,
            request.Type,
            request.Message,
            request.Data);

        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> BroadcastNotification(
        [FromBody] BroadcastNotificationRequest request,
        [FromServices] INotificationService notificationService)
    {
        var result = await notificationService.BroadcastNotificationAsync(
            request.Type,
            request.Message,
            request.Data);

        return result!.ToHttpResult();
    }

    private static async Task<IResult> SendBugNotification(
        [FromBody] SendBugNotificationRequest request,
        [FromServices] ISender sender)
    {
        var command = new SendNotificationCommand(
            request.UserId,
            request.Type,
            request.Message,
            null,
            request.BugId);

        var result = await sender.Send(command);

        return result!.ToHttpResult();
    }
}

public record SendNotificationRequest(
    string UserId,
    string Type,
    string Message,
    string? Data = null);

public record SendNotificationToUserRequest(
    string UserId,
    string Type,
    string Message,
    string? Data = null);

public record BroadcastNotificationRequest(
    string Type,
    string Message,
    string? Data = null);

public record SendBugNotificationRequest(
    string UserId,
    string BugId,
    string Type,
    string Message);
