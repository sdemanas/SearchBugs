using MediatR;
using SearchBugs.Application.Common.Attributes;
using Shared.Results;

namespace SearchBugs.Application.Users.ResetPassword;

public sealed record ResetPasswordCommand(
    [property: AuditIgnore] string Email,
    [property: AuditIgnore] string Token,
    [property: AuditIgnore] string NewPassword
) : IRequest<Result>;
