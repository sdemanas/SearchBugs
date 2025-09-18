using MediatR;
using SearchBugs.Application.Common.Attributes;
using Shared.Results;

namespace SearchBugs.Application.Users.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    [property: AuditIgnore] string CurrentPassword,
    [property: AuditIgnore] string NewPassword) : IRequest<Result>;
