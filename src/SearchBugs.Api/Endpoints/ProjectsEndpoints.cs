using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Projects.CreateProject;
using SearchBugs.Application.Projects.GetProject;
using SearchBugs.Application.Projects.GetProjects;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class ProjectsEndpoints
{
    public record CreateProjectRequest(
        string Name,
        string Description);

    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app.MapGroup("api/projects");
        projects.MapPost("", CreateProject).WithName(nameof(CreateProject)).RequireAuthorization("CreateProject");
        projects.MapGet("", GetProjects).WithName(nameof(GetProjects)).RequireAuthorization("ListAllProjects");
        projects.MapGet("{id:guid}", GetProject).WithName(nameof(GetProject)).RequireAuthorization("ViewProjectDetails");
    }

    public static async Task<IResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        ISender sender)
    {
        var command = new CreateProjectCommand(
            request.Name,
            request.Description
            );

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

}
