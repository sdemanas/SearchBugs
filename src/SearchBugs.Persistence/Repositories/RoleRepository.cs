
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Roles;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Persistence.Repositories;

internal sealed class RoleRepository : IRoleRepository
{

    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Result<Role>> GetByIdAsync(int roleId, CancellationToken cancellationToken)
    {
        var role = Role.GetValues().FirstOrDefault(r => r.Id == roleId);

        if (role == null)
        {
            return Task.FromResult(Result.Failure<Role>(new Error("Role.NotFound", $"Role with ID {roleId} not found")));
        }

        return Task.FromResult(Result.Success(role));
    }

    public Task<Result<Role>> GetByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = Role.GetValues().FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        if (role == null)
        {
            return Task.FromResult(Result.Failure<Role>(new Error("Role.NotFound", $"Role with name '{roleName}' not found")));
        }

        return Task.FromResult(Result.Success(role));
    }

    public async Task<Result<IEnumerable<Role>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _context.Roles.ToListAsync(cancellationToken);
            return Result.Success<IEnumerable<Role>>(roles);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<Role>>(new Error("Role.DatabaseError", $"Failed to retrieve roles: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Permission>>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken)
    {
        // Check if role exists
        var roleExists = Role.GetValues().Any(r => r.Id == roleId);
        if (!roleExists)
        {
            return Result.Failure<IEnumerable<Permission>>(new Error("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        var permissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var permissions = Permission.GetValues()
            .Where(p => permissionIds.Contains(p.Id))
            .ToList();

        return Result.Success<IEnumerable<Permission>>(permissions);
    }

    public async Task<Result> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        // Check if role exists
        var role = Role.GetValues().FirstOrDefault(r => r.Id == roleId);
        if (role == null)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        // Check if permission exists
        var permission = Permission.GetValues().FirstOrDefault(p => p.Id == permissionId);
        if (permission == null)
        {
            return Result.Failure(new Error("Permission.NotFound", $"Permission with ID {permissionId} not found"));
        }

        // Check if the role-permission mapping already exists
        var existingRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (existingRolePermission != null)
        {
            return Result.Failure(new Error("RolePermission.Conflict", "Permission is already assigned to this role"));
        }

        // Create new role-permission mapping
        var rolePermission = new RolePermission(role, permission);
        _context.RolePermissions.Add(rolePermission);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        // Check if role exists
        var roleExists = Role.GetValues().Any(r => r.Id == roleId);
        if (!roleExists)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        // Check if permission exists
        var permissionExists = Permission.GetValues().Any(p => p.Id == permissionId);
        if (!permissionExists)
        {
            return Result.Failure(new Error("Permission.NotFound", $"Permission with ID {permissionId} not found"));
        }

        // Find and remove the role-permission mapping
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission == null)
        {
            return Result.Failure(new Error("RolePermission.NotFound", "Permission is not assigned to this role"));
        }

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<bool>> HasPermissionAsync(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        // Check if role exists
        var roleExists = Role.GetValues().Any(r => r.Id == roleId);
        if (!roleExists)
        {
            return Result.Failure<bool>(new Error("Role.NotFound", $"Role with ID {roleId} not found"));
        }

        // Check if permission exists
        var permissionExists = Permission.GetValues().Any(p => p.Id == permissionId);
        if (!permissionExists)
        {
            return Result.Failure<bool>(new Error("Permission.NotFound", $"Permission with ID {permissionId} not found"));
        }

        var hasPermission = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        return Result.Success(hasPermission);
    }
}
