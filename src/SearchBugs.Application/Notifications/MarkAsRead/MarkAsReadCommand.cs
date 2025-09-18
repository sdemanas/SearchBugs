using Shared.Messaging;

namespace SearchBugs.Application.Notifications.MarkAsRead;

public record MarkAsReadCommand(string NotificationId) : ICommand;
