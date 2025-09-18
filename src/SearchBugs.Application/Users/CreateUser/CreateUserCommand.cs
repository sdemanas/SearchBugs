using MediatR;
using SearchBugs.Application.Common.Attributes;
using Shared.Results;

namespace SearchBugs.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    [property: AuditIgnore] string Password,
    string[]? Roles = null) : IRequest<Result<CreateUserResponse>>;
