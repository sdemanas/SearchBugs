using SearchBugs.Domain.Users;

namespace SearchBugs.Domain.Notifications;

public interface INotificationRepository : IRepository<Notification, NotificationId>
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(UserId userId, CancellationToken cancellationToken = default);
}
