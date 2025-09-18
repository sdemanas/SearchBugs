using Shared.Errors;

namespace SearchBugs.Application.Notifications;

public static class NotificationValidationErrors
{
    public static Error UserIdIsRequired => new("Notification.UserIdIsRequired", "The user ID is required.");
    public static Error InvalidUserId => new("Notification.InvalidUserId", "The user ID is invalid.");
    public static Error TypeIsRequired => new("Notification.TypeIsRequired", "The notification type is required.");
    public static Error TypeMaxLength => new("Notification.TypeMaxLength", "The notification type must not exceed 100 characters.");
    public static Error MessageIsRequired => new("Notification.MessageIsRequired", "The notification message is required.");
    public static Error MessageMaxLength => new("Notification.MessageMaxLength", "The notification message must not exceed 500 characters.");
    public static Error GroupNameIsRequired => new("Notification.GroupNameIsRequired", "The group name is required.");
    public static Error InvalidBugId => new("Notification.InvalidBugId", "The bug ID is invalid.");
    public static Error NotificationNotFound => new("Notification.NotificationNotFound", "The notification was not found.");
    public static Error SignalRConnectionError => new("Notification.SignalRConnectionError", "Failed to send notification via SignalR.");
    public static Error BroadcastError => new("Notification.BroadcastError", "Failed to broadcast notification.");
}
