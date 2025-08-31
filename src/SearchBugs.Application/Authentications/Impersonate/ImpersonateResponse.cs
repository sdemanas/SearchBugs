namespace SearchBugs.Application.Authentications.Impersonate;

public sealed record ImpersonateResponse(string Token, string ImpersonatedUserEmail);
