using MediatR;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.AuditLogs;
using SearchBugs.Domain.Users;
using SearchBugs.Domain.Repositories;
using Shared.Results;

namespace SearchBugs.Application.Users.GetCurrentUserProfile;

internal sealed class GetCurrentUserProfileQueryHandler
    : IRequestHandler<GetCurrentUserProfileQuery, Result<GetCurrentUserProfileResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IProfileRepository _profileRepository;

    public GetCurrentUserProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IProfileRepository profileRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result<GetCurrentUserProfileResponse>> Handle(
        GetCurrentUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<GetCurrentUserProfileResponse>(userResult.Error);
        }

        var user = userResult.Value;

        // Get the user's profile data from the database
        var profileResult = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        var profile = profileResult.Value; // This could be null if no profile exists yet

        // Get recent audit logs for user activity
        var recentAuditLogs = await _auditLogRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        var recentActivity = MapAuditLogsToActivity(recentAuditLogs.Take(10).ToArray());

        // Build response using real profile data or defaults
        var profileResponse = new GetCurrentUserProfileResponse(
            user.Id.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.Email.Value,
            user.Roles.Select(r => r.Name).ToArray(),
            user.CreatedOnUtc,
            user.ModifiedOnUtc,
            // Use real profile data if available, otherwise provide defaults
            Bio: profile?.Bio,
            Location: profile?.Location,
            Website: profile?.Website,
            AvatarUrl: profile?.AvatarUrl ?? GetDefaultAvatarUrl(user.Email.Value),
            Company: profile?.Company,
            JobTitle: profile?.JobTitle,
            TwitterHandle: profile?.TwitterHandle,
            LinkedInProfile: profile?.LinkedInProfile,
            GitHubProfile: profile?.GitHubProfile,
            IsPublic: profile?.IsPublic ?? true,
            DateOfBirth: profile?.DateOfBirth,
            PhoneNumber: profile?.PhoneNumber,
            TimeZone: profile?.TimeZone,
            PreferredLanguage: profile?.PreferredLanguage,
            recentActivity
        );

        return Result.Success(profileResponse);
    }

    private static ProfileActivity[] MapAuditLogsToActivity(AuditLog[] auditLogs)
    {
        return auditLogs.Select(log => new ProfileActivity(
            log.Id.Value.ToString(),
            GetActivityType(log.RequestName),
            GetActivityAction(log.RequestName),
            GetActivityTarget(log.RequestName, log.RequestData),
            log.CreatedOnUtc,
            GetActivityIcon(log.RequestName),
            log.IsSuccess,
            log.Duration
        )).ToArray();
    }

    private static string GetActivityType(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Bug") => "bug",
            var name when name.Contains("Project") => "project",
            var name when name.Contains("User") => "user",
            var name when name.Contains("Role") => "role",
            var name when name.Contains("Login") => "authentication",
            var name when name.Contains("Password") => "security",
            _ => "system"
        };
    }

    private static string GetActivityAction(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Create") => "created",
            var name when name.Contains("Update") => "updated",
            var name when name.Contains("Delete") => "deleted",
            var name when name.Contains("Login") => "logged in",
            var name when name.Contains("Password") => "changed password",
            var name when name.Contains("Assign") => "assigned",
            var name when name.Contains("Remove") => "removed",
            _ => "performed action"
        };
    }

    private static string GetActivityTarget(string requestName, string requestData)
    {
        // Try to extract meaningful target from request name or data
        if (requestName.Contains("Bug"))
            return "bug tracking item";
        if (requestName.Contains("Project"))
            return "project";
        if (requestName.Contains("User"))
            return "user account";
        if (requestName.Contains("Role"))
            return "user role";
        if (requestName.Contains("Login"))
            return "system";
        if (requestName.Contains("Password"))
            return "account security";

        return requestName.Replace("Command", "").Replace("Query", "");
    }

    private static string GetActivityIcon(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Bug") => "Bug",
            var name when name.Contains("Project") => "FolderOpen",
            var name when name.Contains("User") => "User",
            var name when name.Contains("Role") => "Shield",
            var name when name.Contains("Login") => "LogIn",
            var name when name.Contains("Password") => "Lock",
            _ => "Activity"
        };
    }

    private static string GetDefaultAvatarUrl(string email)
    {
        // Use a deterministic avatar service
        var hash = Math.Abs(email.GetHashCode());
        return $"https://avatars.dicebear.com/api/avataaars/{hash}.svg";
    }
}
