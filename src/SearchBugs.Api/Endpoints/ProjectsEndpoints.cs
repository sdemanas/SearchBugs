using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Api.Extensions;
using SearchBugs.Application.Projects.CreateProject;
using SearchBugs.Application.Projects.GetProject;
using SearchBugs.Application.Projects.GetProjects;

namespace SearchBugs.Api.Endpoints;

public static class ProjectsEndpoints
{
    public record CreateProjectRequest(
        string Name,
        string Description);

    public record UpdateProjectRequest(
        string Name,
        string Description);

    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app.MapGroup("api/projects")
            .WithTags("Projects")
            .WithOpenApi();

        // Basic CRUD operations
        projects.MapGet("", GetProjects).WithName(nameof(GetProjects)).RequireAuthorization("ListAllProjects");
        projects.MapGet("{id:guid}", GetProject).WithName(nameof(GetProject)).RequireAuthorization("ViewProjectDetails");
        projects.MapPost("", CreateProject).WithName(nameof(CreateProject)).RequireAuthorization("CreateProject");
        projects.MapPut("{id:guid}", UpdateProject).WithName(nameof(UpdateProject)).RequireAuthorization("UpdateProject");
        projects.MapDelete("{id:guid}", DeleteProject).WithName(nameof(DeleteProject)).RequireAuthorization("DeleteProject");

        // Project statistics and analytics
        projects.MapGet("{id:guid}/statistics", GetProjectStatistics).WithName(nameof(GetProjectStatistics)).RequireAuthorization("ViewProjectDetails");
        projects.MapGet("{id:guid}/bugs", GetProjectBugs).WithName(nameof(GetProjectBugs)).RequireAuthorization("ListAllBugs");
        projects.MapGet("{id:guid}/members", GetProjectMembers).WithName(nameof(GetProjectMembers)).RequireAuthorization("ViewProjectDetails");
        projects.MapPost("{id:guid}/members", AddProjectMember).WithName(nameof(AddProjectMember)).RequireAuthorization("UpdateProject");
        projects.MapDelete("{id:guid}/members/{userId:guid}", RemoveProjectMember).WithName(nameof(RemoveProjectMember)).RequireAuthorization("UpdateProject");
    }

    public static async Task<IResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        ISender sender)
    {
        var command = new CreateProjectCommand(
            request.Name,
            request.Description);

        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetProjects(ISender sender)
    {
        var query = new GetProjectsQuery();
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetProject(Guid id, ISender sender)
    {
        var query = new GetProjectQuery(id);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static Task<IResult> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        ISender sender)
    {
        // Note: This would require implementing UpdateProjectCommand in the Application layer
        return Task.FromResult(Results.Ok($"Update project {id} with name: {request.Name}, description: {request.Description}"));
    }

    public static Task<IResult> DeleteProject(Guid id, ISender sender)
    {
        // Note: This would require implementing DeleteProjectCommand in the Application layer
        return Task.FromResult(Results.Ok($"Delete project {id}"));
    }

    public static Task<IResult> GetProjectStatistics(Guid id, ISender sender)
    {
        // Note: This would require implementing GetProjectStatisticsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get statistics for project {id}"));
    }

    public static Task<IResult> GetProjectBugs(
        Guid id,
        ISender sender,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        // Note: This would require implementing GetProjectBugsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get bugs for project {id} with filters"));
    }

    public static Task<IResult> GetProjectMembers(Guid id, ISender sender)
    {
        // Note: This would require implementing GetProjectMembersQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get members for project {id}"));
    }

    public static Task<IResult> AddProjectMember(
        Guid id,
        [FromBody] AddProjectMemberRequest request,
        ISender sender)
    {
        // Note: This would require implementing AddProjectMemberCommand in the Application layer
        return Task.FromResult(Results.Ok($"Add member {request.UserId} to project {id}"));
    }

    public static Task<IResult> RemoveProjectMember(
        Guid id,
        Guid userId,
        ISender sender)
    {
        // Note: This would require implementing RemoveProjectMemberCommand in the Application layer
        return Task.FromResult(Results.Ok($"Remove member {userId} from project {id}"));
    }

    public record AddProjectMemberRequest(Guid UserId, string Role = "Member");
}
