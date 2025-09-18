namespace SearchBugs.Application.Users.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string[]? Roles,
    DateTime? CreatedOnUtc,
    DateTime? ModifiedOnUtc,
    // Profile fields from domain model
    string? Bio,
    string? Location,
    string? Website,
    string? AvatarUrl,
    string? Company,
    string? JobTitle,
    string? TwitterHandle,
    string? LinkedInProfile,
    string? GitHubProfile,
    bool IsPublic,
    DateTime? DateOfBirth,
    string? PhoneNumber,
    string? TimeZone,
    string? PreferredLanguage,
    ProfileActivity[] RecentActivity
);

public sealed record ProfileActivity(
    string Id,
    string Type,
    string Action,
    string Target,
    DateTime Timestamp,
    string Icon,
    bool IsSuccess,
    TimeSpan Duration
);
