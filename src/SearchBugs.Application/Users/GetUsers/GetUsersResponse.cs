namespace SearchBugs.Application.Users.GetUsers;

public sealed record GetUsersResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string[]? Roles,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
