using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest<Result>;
