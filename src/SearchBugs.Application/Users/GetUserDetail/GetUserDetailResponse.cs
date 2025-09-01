using SearchBugs.Application.Users.Common;

namespace SearchBugs.Application.Users.GetUserDetail;

public sealed record GetUserDetailResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    RoleDto[]? Roles,
    DateTime? CreatedOnUtc,
    DateTime? ModifiedOnUtc);

