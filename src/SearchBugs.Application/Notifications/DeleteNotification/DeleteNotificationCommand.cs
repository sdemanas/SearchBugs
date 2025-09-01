using Shared.Messaging;

namespace SearchBugs.Application.Notifications.DeleteNotification;

public record DeleteNotificationCommand(string NotificationId) : ICommand;
