using MediatR;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Users;
using SearchBugs.Domain;
using Shared.Results;
using Shared.Errors;

namespace SearchBugs.Application.Users.UpdateCurrentUserProfile;

internal sealed class UpdateCurrentUserProfileCommandHandler
    : IRequestHandler<UpdateCurrentUserProfileCommand, Result<UpdateCurrentUserProfileResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrentUserProfileCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateCurrentUserProfileResponse>> Handle(
        UpdateCurrentUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<UpdateCurrentUserProfileResponse>(userResult.Error);
        }

        var user = userResult.Value;

        // Update basic user information (name)
        try
        {
            var name = Name.Create(request.FirstName, request.LastName);
            user.Update(name);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<UpdateCurrentUserProfileResponse>(
                new Error("Name.Invalid", ex.Message));
        }

        // Get or create user profile
        var profileResult = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        Profile profile;

        if (profileResult.IsFailure || profileResult.Value == null)
        {
            // Create new profile if it doesn't exist
            profile = Profile.Create(
                userId,
                request.Bio,
                request.Location,
                request.Website,
                request.AvatarUrl,
                request.Company,
                request.JobTitle,
                request.TwitterHandle,
                request.LinkedInUrl,
                request.GitHubUsername,
                true, // IsPublic - default to true
                request.DateOfBirth,
                request.PhoneNumber,
                request.TimeZone,
                null  // PreferredLanguage - not included in request
            );
        }
        else
        {
            // Update existing profile with all fields
            profile = profileResult.Value;

            // Update basic info
            profile.UpdateBasicInfo(
                request.Bio,
                request.Location,
                request.Website,
                request.Company,
                request.JobTitle
            );

            // Update social media links
            profile.UpdateSocialMedia(
                request.TwitterHandle,
                request.LinkedInUrl,
                request.GitHubUsername
            );

            // Update personal info
            profile.UpdatePersonalInfo(
                request.DateOfBirth,
                request.PhoneNumber,
                request.TimeZone,
                profile.PreferredLanguage // Keep existing preferred language
            );

            // Update avatar if provided
            if (!string.IsNullOrEmpty(request.AvatarUrl))
            {
                profile.UpdateAvatar(request.AvatarUrl);
            }
        }

        // Save profile
        await _profileRepository.CreateOrUpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdateCurrentUserProfileResponse(
            Success: true,
            Message: "Profile updated successfully"
        );

        return Result.Success(response);
    }
}
