using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Domain.Users;

public interface IProfileRepository : IRepository<Profile, ProfileId>
{
    Task<Result<Profile>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<Result<Profile>> CreateOrUpdateAsync(Profile profile, CancellationToken cancellationToken = default);
}
