using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Users.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, string Role) : IRequest<Result>;
