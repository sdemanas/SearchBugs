using Shared.Messaging;

namespace SearchBugs.Application.Users.GetUserActivity;

public sealed record GetUserActivityQuery(
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<GetUserActivityResponse>;
