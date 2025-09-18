using MediatR;
using SearchBugs.Application.Common.Attributes;
using Shared.Results;

namespace SearchBugs.Application.Users.ForgotPassword;

public sealed record ForgotPasswordCommand(
    [property: AuditIgnore] string Email
) : IRequest<Result>;
