using Shared.Messaging;

namespace SearchBugs.Application.Notifications.GetUnreadCount;

public record GetUnreadCountQuery : IQuery<UnreadCountResponse>;
