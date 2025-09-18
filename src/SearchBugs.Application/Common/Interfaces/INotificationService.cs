using SearchBugs.Domain.Notifications;
using Shared.Results;

namespace SearchBugs.Application.Common.Interfaces;

public interface INotificationService
{
    Task<Result> SendNotificationToUserAsync(string userId, string type, string message, string? data = null);
    Task<Result> SendNotificationToGroupAsync(string groupName, string type, string message, string? data = null);
    Task<Result> SendBugNotificationAsync(string userId, Notification notification);
    Task<Result> BroadcastNotificationAsync(string type, string message, string? data = null);
}
