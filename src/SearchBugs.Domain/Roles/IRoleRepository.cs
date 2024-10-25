using Shared.Results;

namespace SearchBugs.Domain.Roles;

public interface IRoleRepository
{
    Task<Result<Role>> GetByIdAsync(int roleId, CancellationToken cancellationToken);

    Task<Result<Role>> GetByNameAsync(string roleName, CancellationToken cancellationToken);
}
