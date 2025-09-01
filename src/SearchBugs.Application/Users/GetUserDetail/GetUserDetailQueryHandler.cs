using Microsoft.EntityFrameworkCore;
using SearchBugs.Application.Users.Common;
using SearchBugs.Domain;
using SearchBugs.Domain.Users;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Users.GetUserDetail;

public sealed class GetUserDetailQueryHandler : IQueryHandler<GetUserDetailQuery, GetUserDetailResponse>
{
    private readonly IApplicationDbContext _context;

    public GetUserDetailQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<GetUserDetailResponse>> Handle(GetUserDetailQuery query, CancellationToken cancellationToken)
    {
        var user = await GetUserDetailByIdAsync(query.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<GetUserDetailResponse>(UserErrors.NotFound(new UserId(query.UserId)));
        }

        return Result.Success(user);
    }

    public async Task<GetUserDetailResponse?> GetUserDetailByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userIdObj = new UserId(userId);
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userIdObj, cancellationToken);

        if (user == null)
        {
            return null;
        }


        return new GetUserDetailResponse(
            user.Id.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.Email.Value,
            user.Roles?.Select(r => new RoleDto(r.Id, r.Name)).ToArray(),
            user.CreatedOnUtc,
            user.ModifiedOnUtc
        );
    }
}