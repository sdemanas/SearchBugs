using SearchBugs.Domain.Users;
using Shared.Primitives;

namespace SearchBugs.Domain.Bugs.Events;

public sealed record BugCreatedDomainEvent(
    BugId BugId,
    string Title,
    string Description,
    UserId AssigneeId,
    UserId ReporterId,
    DateTime CreatedAt
) : DomainEvent(Guid.NewGuid(), DateTime.UtcNow);
