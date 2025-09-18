using Shared.Messaging;

namespace SearchBugs.Application.Authentications.StopImpersonate;

public sealed record StopImpersonateCommand() : ICommand<StopImpersonateResponse>;
