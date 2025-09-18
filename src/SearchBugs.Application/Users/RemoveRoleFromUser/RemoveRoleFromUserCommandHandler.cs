using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.Users.RemoveRoleFromUser;

internal sealed class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleFromUserCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
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

        // Check if user has this role
        var existingRole = user.Roles.FirstOrDefault(r => r.Id == role.Id);
        if (existingRole is null)
        {
            return Result.Failure(new Error("UserRole.NotAssigned", "User does not have this role assigned."));
        }

        // Remove role from user
        user.RemoveRole(existingRole);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
