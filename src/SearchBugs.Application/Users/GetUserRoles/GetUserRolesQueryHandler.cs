using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.Users.GetUserRoles;

internal sealed class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<UserRolesResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserRolesResponse>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == new UserId(request.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserRolesResponse>(UserErrors.NotFound(new UserId(request.UserId)));
        }

        var response = new UserRolesResponse(
            user.Id.Value,
            user.Email.Value,
            $"{user.Name.FirstName} {user.Name.LastName}",
            user.Roles.Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Permissions.Select(p => new PermissionDto(p.Id, p.Name, p.Description)).ToList()
            )).ToList()
        );

        return Result.Success(response);
    }
}
