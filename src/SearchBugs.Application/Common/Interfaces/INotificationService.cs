using SearchBugs.Domain.Notifications;

namespace SearchBugs.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotificationToUserAsync(string userId, string type, string message, string? data = null);
    Task SendNotificationToGroupAsync(string groupName, string type, string message, string? data = null);
    Task SendBugNotificationAsync(string userId, Notification notification);
    Task BroadcastNotificationAsync(string type, string message, string? data = null);
}
