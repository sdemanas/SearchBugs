using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string[]? Roles = null) : IRequest<Result<CreateUserResponse>>;
