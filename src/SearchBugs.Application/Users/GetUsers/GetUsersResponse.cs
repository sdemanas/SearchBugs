namespace SearchBugs.Application.Users.GetUsers;

public record GetUsersResponse(Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
