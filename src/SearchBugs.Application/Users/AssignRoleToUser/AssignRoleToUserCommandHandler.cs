using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Results;
using Shared.Errors;

namespace SearchBugs.Application.Users.AssignRoleToUser;

internal sealed class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleToUserCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        // Get user with existing roles
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == new UserId(request.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(new UserId(request.UserId)));
        }

        // Get role
        var role = Role.FromId(request.RoleId);
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role with ID {request.RoleId} was not found."));
        }

        // Check if user already has this role
        if (user.Roles.Any(r => r.Id == role.Id))
        {
            return Result.Failure(new Error("UserRole.AlreadyAssigned", "User already has this role assigned."));
        }

        // Add role to user
        user.AddRole(role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
