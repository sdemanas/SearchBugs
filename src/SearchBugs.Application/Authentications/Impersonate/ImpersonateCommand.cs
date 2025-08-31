using Shared.Messaging;

namespace SearchBugs.Application.Authentications.Impersonate;

public sealed record ImpersonateCommand(Guid UserIdToImpersonate) : ICommand<ImpersonateResponse>;
