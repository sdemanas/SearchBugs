using SearchBugs.Domain.Users;
using Shared.Primitives;
using Shared.Time;

namespace SearchBugs.Domain.Bugs;

public class BugHistory : Entity<HistoryId>
{
    public BugId BugId { get; set; }
    public UserId ChangedBy { get; set; }
    public string FieldChanged { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public DateTime ChangedAtUtc { get; set; }

    // Navigation property
    public User? User { get; set; }

    private BugHistory()
    {
        BugId = null!;
        ChangedBy = null!;
        FieldChanged = string.Empty;
        OldValue = string.Empty;
        NewValue = string.Empty;
    }

    private BugHistory(HistoryId id, BugId bugId, UserId changedBy, string fieldChanged, string oldValue, string newValue, DateTime changedAt) : base(id)
    {
        BugId = bugId;
        ChangedBy = changedBy;
        FieldChanged = fieldChanged;
        OldValue = oldValue;
        NewValue = newValue;
        ChangedAtUtc = changedAt;
    }

    public static BugHistory Create(BugId bugId, UserId changedBy, string fieldChanged, string oldValue, string newValue)
    {
        var id = new HistoryId(Guid.NewGuid());
        return new BugHistory(id, bugId, changedBy, fieldChanged, oldValue, newValue, SystemTime.UtcNow);
    }
}
