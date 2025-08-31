using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Users;
using Shared.Primitives;
using Shared.Time;

namespace SearchBugs.Domain.Notifications;

public class Notification : Entity<NotificationId>, IAuditable
{
    public UserId UserId { get; }
    public string Type { get; }
    public string Message { get; }
    public BugId BugId { get; }
    public bool IsRead { get; private set; }
    public DateTime CreatedOnUtc { get; }
    public DateTime? ModifiedOnUtc { get; private set; }

    private Notification()
    {
        Type = string.Empty;
        Message = string.Empty;
        UserId = null!;
        BugId = null!;
    }

    private Notification(NotificationId id, UserId userId, string type, string message, BugId bugId, bool isRead, DateTime createdOnUtc) : base(id)
    {
        UserId = userId;
        Type = type;
        Message = message;
        BugId = bugId;
        IsRead = isRead;
        CreatedOnUtc = createdOnUtc;
    }

    public static Notification Create(UserId userId, string type, string message, BugId bugId, bool isRead)
    {
        var id = new NotificationId(Guid.NewGuid());
        return new Notification(id, userId, type, message, bugId, isRead, SystemTime.UtcNow);
    }
}
