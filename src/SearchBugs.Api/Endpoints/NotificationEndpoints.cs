using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Notifications;
using SearchBugs.Domain.Users;

namespace SearchBugs.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapPost("/send", SendNotification).RequireAuthorization("ViewNotification");
        group.MapPost("/send-to-user", SendNotificationToUser).RequireAuthorization("ViewNotification");
        group.MapPost("/broadcast", BroadcastNotification).RequireAuthorization("ViewNotification");
        group.MapPost("/bug-notification", SendBugNotification).RequireAuthorization("ViewNotification");
    }

    private static async Task<IResult> SendNotification(
        [FromBody] SendNotificationRequest request,
        [FromServices] INotificationService notificationService)
    {
        await notificationService.SendNotificationToUserAsync(
            request.UserId,
            request.Type,
            request.Message,
            request.Data);

        return Results.Ok(new { Message = "Notification sent successfully" });
    }

    private static async Task<IResult> SendNotificationToUser(
        [FromBody] SendNotificationToUserRequest request,
        [FromServices] INotificationService notificationService)
    {
        await notificationService.SendNotificationToUserAsync(
            request.UserId,
            request.Type,
            request.Message,
            request.Data);

        return Results.Ok(new { Message = "Notification sent to user successfully" });
    }

    private static async Task<IResult> BroadcastNotification(
        [FromBody] BroadcastNotificationRequest request,
        [FromServices] INotificationService notificationService)
    {
        await notificationService.BroadcastNotificationAsync(
            request.Type,
            request.Message,
            request.Data);

        return Results.Ok(new { Message = "Notification broadcasted successfully" });
    }

    private static async Task<IResult> SendBugNotification(
        [FromBody] SendBugNotificationRequest request,
        [FromServices] INotificationService notificationService)
    {
        var notification = Notification.Create(
            new UserId(Guid.Parse(request.UserId)),
            request.Type,
            request.Message,
            new Domain.Bugs.BugId(Guid.Parse(request.BugId)),
            false
        );

        await notificationService.SendBugNotificationAsync(request.UserId, notification);

        return Results.Ok(new { Message = "Bug notification sent successfully" });
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
