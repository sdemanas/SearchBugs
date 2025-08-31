using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Users.GetCurrentUserProfile;
using SearchBugs.Application.Users.GetUserActivity;
using SearchBugs.Application.Users.UpdateCurrentUserProfile;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class ProfileEndpoints
{
    public record UpdateProfileRequest(
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
    );

    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var profile = app.MapGroup("api/profile")
            .WithTags("Profile")
            .WithOpenApi()
            .RequireAuthorization(); // All profile endpoints require authentication

        profile.MapGet("/", GetCurrentUserProfile)
            .WithName(nameof(GetCurrentUserProfile))
            .WithDescription("Get current user's profile information");

        profile.MapGet("/activity", GetUserActivity)
            .WithName(nameof(GetUserActivity))
            .WithDescription("Get current user's activity from audit logs");

        profile.MapPut("/", UpdateCurrentUserProfile)
            .WithName(nameof(UpdateCurrentUserProfile))
            .WithDescription("Update current user's profile information");
    }

    public static async Task<IResult> GetCurrentUserProfile(ISender sender)
    {
        var query = new GetCurrentUserProfileQuery();
        var result = await sender.Send(query);
        return result.ToHttpResult();
    }

    public static async Task<IResult> GetUserActivity(
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetUserActivityQuery(pageNumber, pageSize);
        var result = await sender.Send(query);
        return result.ToHttpResult();
    }

    public static async Task<IResult> UpdateCurrentUserProfile(
        [FromBody] UpdateProfileRequest request,
        ISender sender)
    {
        var command = new UpdateCurrentUserProfileCommand(
            request.FirstName,
            request.LastName,
            request.Bio,
            request.Location,
            request.Website,
            request.GitHubUsername,
            request.LinkedInUrl,
            request.TwitterHandle,
            request.Company,
            request.JobTitle,
            request.PhoneNumber,
            request.TimeZone,
            request.AvatarUrl,
            request.DateOfBirth
        );

        var result = await sender.Send(command);
        return result.ToHttpResult();
    }
}
