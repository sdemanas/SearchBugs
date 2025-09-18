using SearchBugs.Domain.Users;
using Shared.Primitives;

namespace SearchBugs.Domain.Bugs.Events;

public sealed record BugAssignedDomainEvent(
    BugId BugId,
    string Title,
    UserId AssigneeId,
    UserId AssignedBy,
    DateTime AssignedAt
) : DomainEvent(Guid.NewGuid(), DateTime.UtcNow);
