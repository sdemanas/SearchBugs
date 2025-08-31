using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.BugTracking.Attachments;
using SearchBugs.Application.BugTracking.Comments;
using SearchBugs.Application.BugTracking.Create;
using SearchBugs.Application.BugTracking.CustomFields;
using SearchBugs.Application.BugTracking.Delete;
using SearchBugs.Application.BugTracking.GetBugById;
using SearchBugs.Application.BugTracking.GetBugs;
using SearchBugs.Application.BugTracking.History;
using SearchBugs.Application.BugTracking.TimeTracking;
using SearchBugs.Application.BugTracking.Update;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using SearchBugs.Api.Extensions;
using Shared.Results;

namespace SearchBugs.Api.Endpoints;

public static class BugsEndpoints
{
    public record CreateBugRequest(
        string Title,
        string Description,
        string Status,
        string Priority,
        string Severity,
        Guid ProjectId,
        Guid AssigneeId,
        Guid ReporterId);

    public record UpdateBugRequest(
        string Title,
        string Description,
        string Status,
        string Priority,
        string Severity,
        Guid? AssigneeId);

    public record AddCommentRequest(string Content);
    public record AddAttachmentRequest(IFormFile File);
    public record AddCustomFieldRequest(string Name, string Value);
    public record TrackTimeRequest(TimeSpan Duration, string Description);

    public static void MapBugsEndpoints(this IEndpointRouteBuilder app)
    {
        var bugs = app.MapGroup("api/bugs")
            .WithTags("Bugs")
            .WithOpenApi();

        // Basic CRUD operations
        bugs.MapGet("", GetBugs).WithName(nameof(GetBugs));
        bugs.MapGet("{bugId:guid}", GetBugById).WithName(nameof(GetBugById));
        bugs.MapPost("", CreateBug).WithName(nameof(CreateBug));
        bugs.MapPut("{bugId:guid}", UpdateBug).WithName(nameof(UpdateBug));
        bugs.MapDelete("{bugId:guid}", DeleteBug).WithName(nameof(DeleteBug));

        // Comments
        bugs.MapPost("{bugId:guid}/comments", AddComment).WithName(nameof(AddComment));
        bugs.MapGet("{bugId:guid}/comments", GetComments).WithName(nameof(GetComments));

        // Attachments
        bugs.MapPost("{bugId:guid}/attachments", AddAttachment).WithName(nameof(AddAttachment));
        bugs.MapGet("{bugId:guid}/attachments", GetAttachments).WithName(nameof(GetAttachments));

        // History
        bugs.MapGet("{bugId:guid}/history", GetBugHistory).WithName(nameof(GetBugHistory));

        // Time tracking
        bugs.MapPost("{bugId:guid}/time-tracking", TrackTime).WithName(nameof(TrackTime));
        bugs.MapGet("{bugId:guid}/time-tracking", GetTimeTracking).WithName(nameof(GetTimeTracking));

        // Custom fields
        bugs.MapPost("{bugId:guid}/custom-fields", AddCustomField).WithName(nameof(AddCustomField));
        bugs.MapGet("{bugId:guid}/custom-fields", GetCustomFields).WithName(nameof(GetCustomFields));
    }

    public static async Task<IResult> GetBugs([FromQuery] Guid? projectId, ISender sender)
    {
        var query = new GetBugsQuery(projectId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> CreateBug(
           [FromBody] CreateBugRequest request,
            ISender sender)
    {
        var command = new CreateBugCommand(
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.Severity,
            request.ProjectId,
            request.AssigneeId,
            request.ReporterId);

        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetBugById(Guid bugId, ISender sender)
    {
        var query = new GetBugByIdQuery(bugId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> UpdateBug(
        Guid bugId,
        [FromBody] UpdateBugRequest request,
        ISender sender)
    {
        var command = new UpdateBugCommand(
            bugId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.Severity,
            request.AssigneeId);

        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> DeleteBug(Guid bugId, ISender sender)
    {
        var command = new DeleteBugCommand(bugId);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> AddComment(
        Guid bugId,
        [FromBody] AddCommentRequest request,
        ISender sender)
    {
        var command = new AddCommentCommand(bugId, request.Content);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetComments(Guid bugId, ISender sender)
    {
        var query = new GetBugCommentsQuery(bugId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> AddAttachment(
        Guid bugId,
        [FromForm] AddAttachmentRequest request,
        ISender sender)
    {
        var command = new AddAttachmentCommand(bugId, request.File);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetAttachments(Guid bugId, ISender sender)
    {
        var query = new GetBugAttachmentsQuery(bugId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetBugHistory(Guid bugId, ISender sender)
    {
        var query = new GetBugHistoryQuery(bugId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> TrackTime(
        Guid bugId,
        [FromBody] TrackTimeRequest request,
        ISender sender)
    {
        var command = new TrackTimeCommand(bugId, request.Duration, request.Description);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetTimeTracking(Guid bugId, ISender sender)
    {
        var query = new GetBugTimeTrackingQuery(bugId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> AddCustomField(
        Guid bugId,
        [FromBody] AddCustomFieldRequest request,
        ISender sender)
    {
        var command = new AddCustomFieldCommand(bugId, request.Name, request.Value);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetCustomFields(Guid bugId, ISender sender)
    {
        var query = new GetBugCustomFieldsQuery(bugId);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }
}
