using Shared.Primitives;

namespace SearchBugs.Domain.Users;

public sealed class Profile : Entity<ProfileId>, IAuditable
{
    public UserId UserId { get; private set; }
    public string? Bio { get; private set; }
    public string? Location { get; private set; }
    public string? Website { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Company { get; private set; }
    public string? JobTitle { get; private set; }
    public string? TwitterHandle { get; private set; }
    public string? LinkedInProfile { get; private set; }
    public string? GitHubProfile { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? TimeZone { get; private set; }
    public string? PreferredLanguage { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    // Audit fields
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? ModifiedOnUtc { get; private set; }

    private Profile(
        ProfileId id,
        UserId userId,
        string? bio = null,
        string? location = null,
        string? website = null,
        string? avatarUrl = null,
        string? company = null,
        string? jobTitle = null,
        string? twitterHandle = null,
        string? linkedInProfile = null,
        string? gitHubProfile = null,
        bool isPublic = true,
        DateTime? dateOfBirth = null,
        string? phoneNumber = null,
        string? timeZone = null,
        string? preferredLanguage = null) : base(id)
    {
        UserId = userId;
        Bio = bio;
        Location = location;
        Website = website;
        AvatarUrl = avatarUrl;
        Company = company;
        JobTitle = jobTitle;
        TwitterHandle = twitterHandle;
        LinkedInProfile = linkedInProfile;
        GitHubProfile = gitHubProfile;
        IsPublic = isPublic;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        TimeZone = timeZone;
        PreferredLanguage = preferredLanguage;
        CreatedOnUtc = DateTime.UtcNow;
    }

    // Parameterless constructor for EF
    private Profile() { }

    public static Profile Create(
        UserId userId,
        string? bio = null,
        string? location = null,
        string? website = null,
        string? avatarUrl = null,
        string? company = null,
        string? jobTitle = null,
        string? twitterHandle = null,
        string? linkedInProfile = null,
        string? gitHubProfile = null,
        bool isPublic = true,
        DateTime? dateOfBirth = null,
        string? phoneNumber = null,
        string? timeZone = null,
        string? preferredLanguage = null)
    {
        return new Profile(
            new ProfileId(Guid.NewGuid()),
            userId,
            bio,
            location,
            website,
            avatarUrl,
            company,
            jobTitle,
            twitterHandle,
            linkedInProfile,
            gitHubProfile,
            isPublic,
            dateOfBirth,
            phoneNumber,
            timeZone,
            preferredLanguage);
    }

    public void UpdateBasicInfo(
        string? bio,
        string? location,
        string? website,
        string? company,
        string? jobTitle)
    {
        Bio = bio;
        Location = location;
        Website = website;
        Company = company;
        JobTitle = jobTitle;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public void UpdateSocialMedia(
        string? twitterHandle,
        string? linkedInProfile,
        string? gitHubProfile)
    {
        TwitterHandle = twitterHandle;
        LinkedInProfile = linkedInProfile;
        GitHubProfile = gitHubProfile;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(
        DateTime? dateOfBirth,
        string? phoneNumber,
        string? timeZone,
        string? preferredLanguage)
    {
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        TimeZone = timeZone;
        PreferredLanguage = preferredLanguage;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public void UpdatePrivacy(bool isPublic)
    {
        IsPublic = isPublic;
        ModifiedOnUtc = DateTime.UtcNow;
    }
}
