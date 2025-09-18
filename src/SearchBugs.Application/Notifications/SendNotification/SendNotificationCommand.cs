using Shared.Messaging;

namespace SearchBugs.Application.Notifications.SendNotification;

public record SendNotificationCommand(
    string UserId,
    string Type,
    string Message,
    string? Data = null,
    string? BugId = null) : ICommand;
