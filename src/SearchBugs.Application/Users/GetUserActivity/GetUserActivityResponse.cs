namespace SearchBugs.Application.Users.GetUserActivity;

public sealed record GetUserActivityResponse(
    UserActivityItem[] Activities,
    int TotalCount,
    int PageNumber,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage
);

public sealed record UserActivityItem(
    string Id,
    string Type,
    string Action,
    string Target,
    string Description,
    DateTime Timestamp,
    string Icon,
    bool IsSuccess,
    TimeSpan Duration,
    string? ErrorMessage
);
