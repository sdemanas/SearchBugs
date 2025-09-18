using Shared.Messaging;

namespace SearchBugs.Application.Notifications.GetUnreadNotifications;

public record GetUnreadNotificationsQuery : IQuery<List<NotificationResponse>>;
