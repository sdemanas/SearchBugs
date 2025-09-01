using Shared.Messaging;

namespace SearchBugs.Application.Notifications.GetUserNotifications;

public record GetUserNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? IsRead = null) : IQuery<PagedNotificationResponse>;
