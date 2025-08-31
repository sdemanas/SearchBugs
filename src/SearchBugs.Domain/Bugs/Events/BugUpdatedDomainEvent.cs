using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Users;
using Shared.Primitives;

namespace SearchBugs.Domain.Bugs.Events;

public sealed record BugUpdatedDomainEvent(
    BugId BugId,
    string Title,
    string Description,
    UserId AssigneeId,
    UserId UpdatedBy,
    DateTime UpdatedAt,
    string UpdateType
) : DomainEvent(Guid.NewGuid(), DateTime.UtcNow);
