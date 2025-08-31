using Shared.Messaging;

namespace SearchBugs.Application.Users.GetUsers;

public sealed record GetUsersQuery(
    string? SearchTerm = null,
    string? RoleFilter = null,
    int PageNumber = 1,
    int PageSize = 50
) : IQuery<List<GetUsersResponse>>;

