using Shared.Primitives;

namespace SearchBugs.Domain.Users;

public sealed record ProfileId(Guid Value) : IEntityId
{
    public static implicit operator Guid(ProfileId profileId) => profileId.Value;
    public static implicit operator ProfileId(Guid value) => new(value);
}
