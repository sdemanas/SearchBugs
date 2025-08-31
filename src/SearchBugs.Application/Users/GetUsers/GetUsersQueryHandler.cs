using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Users.GetUsers;

internal sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, List<GetUsersResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<GetUsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var result = await Result.Create(request)
            .Bind(async query => Result.Create(await GetUsersAsync(query, cancellationToken)))
            .Map(users => users.ToList());

        return result;
    }

    private async Task<IEnumerable<GetUsersResponse>?> GetUsersAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = _context.Users
            .Include(u => u.Roles)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            usersQuery = usersQuery.Where(u =>
                u.Name.FirstName.ToLower().Contains(searchTerm) ||
                u.Name.LastName.ToLower().Contains(searchTerm) ||
                u.Email.Value.ToLower().Contains(searchTerm));
        }

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(query.RoleFilter) && query.RoleFilter != "all")
        {
            usersQuery = usersQuery.Where(u => u.Roles.Any(r => r.Name == query.RoleFilter));
        }

        // Apply pagination and ordering
        var users = await usersQuery
            .OrderByDescending(u => u.CreatedOnUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new GetUsersResponse(
                u.Id.Value,
                u.Name.FirstName,
                u.Name.LastName,
                u.Email.Value,
                u.Roles.Select(r => r.Name).ToArray(),
                u.CreatedOnUtc,
                u.ModifiedOnUtc))
            .ToListAsync(cancellationToken);

        return users;
    }
}
