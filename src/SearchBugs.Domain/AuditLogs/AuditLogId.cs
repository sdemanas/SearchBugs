using Shared.Primitives;

namespace SearchBugs.Domain.AuditLogs;

public sealed class AuditLogId : ValueObject, IEntityId
{
    public Guid Value { get; private set; }

    public AuditLogId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditLogId cannot be empty", nameof(value));

        Value = value;
    }

    private AuditLogId() { }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static implicit operator Guid(AuditLogId auditLogId) => auditLogId.Value;
    public static explicit operator AuditLogId(Guid value) => new(value);
}
