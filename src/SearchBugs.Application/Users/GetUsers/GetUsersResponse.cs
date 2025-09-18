using SearchBugs.Application.Users.Common;

namespace SearchBugs.Application.Users.GetUsers;

public sealed record GetUsersResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    RoleDto[]? Roles,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
