using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Common.Interfaces;

namespace SearchBugs.Api.Endpoints;

public static class TestNotificationEndpoints
{
    public static void MapTestNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/test-notifications")
            .WithTags("Test Notifications");

        group.MapPost("/send-test-notification", SendTestNotification);
    }

    private static async Task<IResult> SendTestNotification(
        [FromBody] TestNotificationRequest request,
        [FromServices] INotificationService notificationService)
    {
        var result = await notificationService.SendNotificationToUserAsync(
            request.UserId,
            "test",
            request.Message ?? "This is a test notification!",
            "Test data from backend");

        return result.IsSuccess
            ? Results.Ok(new { Message = "Test notification sent successfully" })
            : Results.BadRequest(result.Error);
    }
}

public record TestNotificationRequest(string UserId, string? Message = null);
