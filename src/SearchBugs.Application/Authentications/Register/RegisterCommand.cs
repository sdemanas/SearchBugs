
using SearchBugs.Application.Common.Attributes;
using Shared.Messaging;

namespace SearchBugs.Application.Authentications.Register;

public sealed record RegisterCommand(
    string Email,
    [property: AuditIgnore] string Password,
    string FirstName,
    string LastName
    ) : ICommand;