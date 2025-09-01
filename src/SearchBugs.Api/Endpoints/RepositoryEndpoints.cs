using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class RepositoryEndpoints
{
    public record CreateRepositoryRequest(
        string Name,
        string Url,
        string Description,
        Guid ProjectId,
        string Branch = "main");

    public record UpdateRepositoryRequest(
        string Name,
        string Url,
        string Description,
        string Branch);

    public record LinkBugToRepositoryRequest(
        Guid BugId,
        string CommitHash,
        string? Description = null);

    public static void MapRepositoryEndpoints(this IEndpointRouteBuilder app)
    {
        var repositories = app.MapGroup("api/repositories")
            .WithTags("Repositories")
            .WithOpenApi();

        // Basic CRUD operations
        repositories.MapGet("", GetAllRepositories).WithName(nameof(GetAllRepositories)).RequireAuthorization("ListAllRepositories");
        repositories.MapGet("{id:guid}", GetRepositoryById).WithName(nameof(GetRepositoryById)).RequireAuthorization("ViewRepositoryDetails");
        repositories.MapPost("", CreateRepository).WithName(nameof(CreateRepository)).RequireAuthorization("CreateRepository");
        repositories.MapPut("{id:guid}", UpdateRepository).WithName(nameof(UpdateRepository)).RequireAuthorization("UpdateRepository");
        repositories.MapDelete("{id:guid}", DeleteRepository).WithName(nameof(DeleteRepository)).RequireAuthorization("DeleteRepository");

        // Repository-bug linking
        repositories.MapPost("{id:guid}/bugs", LinkBugToRepository).WithName(nameof(LinkBugToRepository)).RequireAuthorization("LinkBugToRepository");
        repositories.MapGet("{id:guid}/bugs", GetRepositoryBugs).WithName(nameof(GetRepositoryBugs)).RequireAuthorization("ViewBugRepository");
        repositories.MapDelete("{id:guid}/bugs/{bugId:guid}", UnlinkBugFromRepository).WithName(nameof(UnlinkBugFromRepository)).RequireAuthorization("LinkBugToRepository");

        // Repository analytics and statistics
        repositories.MapGet("{id:guid}/statistics", GetRepositoryStatistics).WithName(nameof(GetRepositoryStatistics)).RequireAuthorization("ViewRepositoryDetails");
        repositories.MapGet("{id:guid}/commits", GetRepositoryCommits).WithName(nameof(GetRepositoryCommits)).RequireAuthorization("ViewRepositoryDetails");
        repositories.MapGet("{id:guid}/branches", GetRepositoryBranches).WithName(nameof(GetRepositoryBranches)).RequireAuthorization("ViewRepositoryDetails");

        // Repository synchronization
        repositories.MapPost("{id:guid}/sync", SyncRepository).WithName(nameof(SyncRepository)).RequireAuthorization("UpdateRepository");
        repositories.MapGet("{id:guid}/sync-status", GetRepositorySyncStatus).WithName(nameof(GetRepositorySyncStatus)).RequireAuthorization("ViewRepositoryDetails");
    }

    public static Task<IResult> GetAllRepositories(
        ISender sender,
        [FromQuery] Guid? projectId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        // Note: This would require implementing GetAllRepositoriesQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get all repositories for project {projectId}"));
    }

    public static Task<IResult> GetRepositoryById(Guid id, ISender sender)
    {
        // Note: This would require implementing GetRepositoryDetailsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get repository details for {id}"));
    }

    public static Task<IResult> CreateRepository(
        [FromBody] CreateRepositoryRequest request,
        ISender sender)
    {
        // Note: This would require implementing CreateRepositoryCommand in the Application layer
        return Task.FromResult(Results.Ok($"Create repository {request.Name} for project {request.ProjectId}"));
    }

    public static Task<IResult> UpdateRepository(
        Guid id,
        [FromBody] UpdateRepositoryRequest request,
        ISender sender)
    {
        // Note: This would require implementing UpdateRepositoryCommand in the Application layer
        return Task.FromResult(Results.Ok($"Update repository {id} with name: {request.Name}"));
    }

    public static Task<IResult> DeleteRepository(Guid id, ISender sender)
    {
        // Note: This would require implementing DeleteRepositoryCommand in the Application layer
        return Task.FromResult(Results.Ok($"Delete repository {id}"));
    }

    public static Task<IResult> LinkBugToRepository(
        Guid id,
        [FromBody] LinkBugToRepositoryRequest request,
        ISender sender)
    {
        // Note: This would require implementing LinkBugToRepositoryCommand in the Application layer
        return Task.FromResult(Results.Ok($"Link bug {request.BugId} to repository {id} with commit {request.CommitHash}"));
    }

    public static Task<IResult> GetRepositoryBugs(
        Guid id,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        // Note: This would require implementing GetRepositoryBugsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get bugs linked to repository {id}"));
    }

    public static Task<IResult> UnlinkBugFromRepository(
        Guid id,
        Guid bugId,
        ISender sender)
    {
        // Note: This would require implementing UnlinkBugFromRepositoryCommand in the Application layer
        return Task.FromResult(Results.Ok($"Unlink bug {bugId} from repository {id}"));
    }

    public static Task<IResult> GetRepositoryStatistics(Guid id, ISender sender)
    {
        // Note: This would require implementing GetRepositoryStatisticsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get statistics for repository {id}"));
    }

    public static Task<IResult> GetRepositoryCommits(
        Guid id,
        ISender sender,
        [FromQuery] string? branch = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        // Note: This would require implementing GetRepositoryCommitsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get commits for repository {id} on branch {branch ?? "main"}"));
    }

    public static Task<IResult> GetRepositoryBranches(Guid id, ISender sender)
    {
        // Note: This would require implementing GetRepositoryBranchesQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get branches for repository {id}"));
    }

    public static Task<IResult> SyncRepository(Guid id, ISender sender)
    {
        // Note: This would require implementing SyncRepositoryCommand in the Application layer
        return Task.FromResult(Results.Ok($"Sync repository {id} with remote"));
    }

    public static Task<IResult> GetRepositorySyncStatus(Guid id, ISender sender)
    {
        // Note: This would require implementing GetRepositorySyncStatusQuery in the Application layer
        return Task.FromResult(Results.Ok($"Get sync status for repository {id}"));
    }
}
