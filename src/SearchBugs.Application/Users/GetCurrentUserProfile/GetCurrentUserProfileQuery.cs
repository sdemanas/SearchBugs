using Shared.Messaging;

namespace SearchBugs.Application.Users.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileQuery() : IQuery<GetCurrentUserProfileResponse>;
