using Shared.Results;

namespace SearchBugs.Domain.Roles;

public interface IRoleRepository
{
    Task<Result<Role>> GetByIdAsync(int roleId, CancellationToken cancellationToken);
    Task<Result<Role>> GetByNameAsync(string roleName, CancellationToken cancellationToken);
    Task<Result<IEnumerable<Role>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<IEnumerable<Permission>>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken);
    Task<Result> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken);
    Task<Result> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken);
    Task<Result<bool>> HasPermissionAsync(int roleId, int permissionId, CancellationToken cancellationToken);
}
