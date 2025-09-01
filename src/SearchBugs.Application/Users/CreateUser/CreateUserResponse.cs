using SearchBugs.Application.Users.Common;

namespace SearchBugs.Application.Users.CreateUser;

public record CreateUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    RoleDto[] Roles,
    DateTime CreatedOnUtc);
