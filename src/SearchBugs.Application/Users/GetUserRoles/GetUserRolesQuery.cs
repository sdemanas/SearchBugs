using MediatR;
using Shared.Results;

namespace SearchBugs.Application.Users.GetUserRoles;

public record GetUserRolesQuery(Guid UserId) : IRequest<Result<UserRolesResponse>>;
