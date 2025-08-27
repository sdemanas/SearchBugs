namespace SearchBugs.Application.Users.CreateUser;

public record CreateUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string[] Roles,
    DateTime CreatedOnUtc);
