using SearchBugs.Application.Common.Attributes;
using Shared.Messaging;

namespace SearchBugs.Application.Users.UpdateCurrentUserProfile;

public sealed record UpdateCurrentUserProfileCommand(
    string FirstName,
    string LastName,
    string? Bio,
    string? Location,
    string? Website,
    string? GitHubUsername,
    string? LinkedInUrl,
    string? TwitterHandle,
    string? Company,
    string? JobTitle,
    string? PhoneNumber,
    string? TimeZone,
    string? AvatarUrl,
    DateTime? DateOfBirth
) : ICommand<UpdateCurrentUserProfileResponse>;
