namespace SearchBugs.Domain.Users;

public interface IJwtProvider
{
    string GenerateJwtToken(User user);
    string GenerateImpersonationJwtToken(User originalUser, User impersonatedUser);
}
