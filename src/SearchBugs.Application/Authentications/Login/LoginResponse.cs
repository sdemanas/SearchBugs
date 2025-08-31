using SearchBugs.Application.Common.Attributes;

namespace SearchBugs.Application.Authentications.Login;

internal sealed record LoginResponse([property: AuditIgnore] string Token);
