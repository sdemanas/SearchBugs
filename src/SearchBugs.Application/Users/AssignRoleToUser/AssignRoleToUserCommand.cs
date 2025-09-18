using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Users.AssignRoleToUser;

public record AssignRoleToUserCommand(Guid UserId, int RoleId) : IRequest<Result>;
