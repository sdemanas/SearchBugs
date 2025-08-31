using Microsoft.AspNetCore.SignalR;
using SearchBugs.Api.Hubs;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Notifications;

namespace SearchBugs.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(string userId, string type, string message, string? data = null)
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
    }

    public async Task SendNotificationToGroupAsync(string groupName, string type, string message, string? data = null)
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
    }

    public async Task SendBugNotificationAsync(string userId, Notification notification)
    {
        var notificationData = new
        {
            Id = notification.Id.Value,
            Type = notification.Type,
            Message = notification.Message,
            BugId = notification.BugId.Value,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"User_{userId}")
            .SendAsync("ReceiveBugNotification", notificationData);
    }

    public async Task BroadcastNotificationAsync(string type, string message, string? data = null)
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
    }
}
