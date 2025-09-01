using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Users.RemoveRoleFromUser;

public record RemoveRoleFromUserCommand(Guid UserId, int RoleId) : IRequest<Result>;
