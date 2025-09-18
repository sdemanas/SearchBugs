using Microsoft.AspNetCore.SignalR;
using SearchBugs.Api.Hubs;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Application.Notifications;
using SearchBugs.Domain.Notifications;
using Shared.Results;

namespace SearchBugs.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<Result> SendNotificationToUserAsync(string userId, string type, string message, string? data = null)
    {
        // Validate input
        if (string.IsNullOrEmpty(userId))
            return Result.Failure(NotificationValidationErrors.UserIdIsRequired);

        if (string.IsNullOrEmpty(type))
            return Result.Failure(NotificationValidationErrors.TypeIsRequired);

        if (type.Length > 100)
            return Result.Failure(NotificationValidationErrors.TypeMaxLength);

        if (string.IsNullOrEmpty(message))
            return Result.Failure(NotificationValidationErrors.MessageIsRequired);

        if (message.Length > 500)
            return Result.Failure(NotificationValidationErrors.MessageMaxLength);

        try
        {
            var notificationData = new
            {
                Type = type,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notificationData);

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(NotificationValidationErrors.SignalRConnectionError);
        }
    }

    public async Task<Result> SendNotificationToGroupAsync(string groupName, string type, string message, string? data = null)
    {
        // Validate input
        if (string.IsNullOrEmpty(groupName))
            return Result.Failure(NotificationValidationErrors.GroupNameIsRequired);

        if (string.IsNullOrEmpty(type))
            return Result.Failure(NotificationValidationErrors.TypeIsRequired);

        if (type.Length > 100)
            return Result.Failure(NotificationValidationErrors.TypeMaxLength);

        if (string.IsNullOrEmpty(message))
            return Result.Failure(NotificationValidationErrors.MessageIsRequired);

        if (message.Length > 500)
            return Result.Failure(NotificationValidationErrors.MessageMaxLength);

        try
        {
            var notificationData = new
            {
                Type = type,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification", notificationData);

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(NotificationValidationErrors.SignalRConnectionError);
        }
    }

    public async Task<Result> SendBugNotificationAsync(string userId, Notification notification)
    {
        // Validate input
        if (string.IsNullOrEmpty(userId))
            return Result.Failure(NotificationValidationErrors.UserIdIsRequired);

        if (notification == null)
            return Result.Failure(NotificationValidationErrors.NotificationNotFound);

        try
        {
            var notificationData = new
            {
                Id = notification.Id.Value,
                Type = notification.Type,
                Message = notification.Message,
                BugId = notification.BugId.Value,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedOnUtc,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveBugNotification", notificationData);

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(NotificationValidationErrors.SignalRConnectionError);
        }
    }

    public async Task<Result> BroadcastNotificationAsync(string type, string message, string? data = null)
    {
        // Validate input
        if (string.IsNullOrEmpty(type))
            return Result.Failure(NotificationValidationErrors.TypeIsRequired);

        if (type.Length > 100)
            return Result.Failure(NotificationValidationErrors.TypeMaxLength);

        if (string.IsNullOrEmpty(message))
            return Result.Failure(NotificationValidationErrors.MessageIsRequired);

        if (message.Length > 500)
            return Result.Failure(NotificationValidationErrors.MessageMaxLength);

        try
        {
            var notificationData = new
            {
                Type = type,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notificationData);

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(NotificationValidationErrors.BroadcastError);
        }
    }
}
