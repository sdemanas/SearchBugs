using SearchBugs.Application.Common.Attributes;
using Shared.Messaging;

namespace SearchBugs.Application.Authentications.Login;

public sealed record LoginCommand(string Email, [property: AuditIgnore] string Password) : ICommand<LoginResponse>;
