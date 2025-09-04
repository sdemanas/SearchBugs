using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Git.CommitChanges;
using SearchBugs.Application.Git.CreateGitRepo;
using SearchBugs.Application.Git.DeleteGitRepo;
using SearchBugs.Application.Git.GetCommitDiff;
using SearchBugs.Application.Git.GetGitRepo;
using SearchBugs.Application.Git.GetGitReposDetails;
using SearchBugs.Application.Git.GetListTree;
using SearchBugs.Application.Git.GitHttpServer;
using SearchBugs.Application.Git.GetFileContents;
using SearchBugs.Application.Git.GetBranches;
using SearchBugs.Application.Git.CloneRepository;
using SearchBugs.Api.Extensions;
using Shared.Results;


namespace SearchBugs.Api.Endpoints;

public static class RepoEndpoints
{
    public record CreateGitRepositoryRequest(string Name, string Description, string Url, Guid ProjectId);

    public static void MapRepoEndpoints(this WebApplication app)
    {
        app
            .MapMethods("{name}.git/{**path}", new[] { "GET", "POST", "PUT" }, [ApiExplorerSettings(IgnoreApi = true)] async (
        ISender sender,
            HttpContext httpContext,
            string name,
            string? path,
            CancellationToken cancellationToken) =>
            {
                var command = new GitHttpServerCommand(
                 name,
                 path ?? string.Empty,
                httpContext
            );
                await sender.Send(command, cancellationToken);
            }).ExcludeFromDescription();

        var repo = app.MapGroup("api/repo");
        repo.MapGet("", GetRepositories).WithName("GetGitRepositories").RequireAuthorization("ListAllRepositories");
        repo.MapGet("{url}/{path}", GetRepositoryDetails).WithName("GetGitRepositoryDetails").RequireAuthorization("ViewRepositoryDetails");
        repo.MapPost("", CreateRepository).WithName("CreateGitRepository").RequireAuthorization("CreateRepository");
        repo.MapDelete("{url}", DeleteRepository).WithName("DeleteGitRepository").RequireAuthorization("DeleteRepository");
        repo.MapGet("{url}/commit/{commitSha}", GetCommitDiff).WithName("GetGitCommitDiff").RequireAuthorization("ViewRepositoryDetails");
        repo.MapPost("{url}/commit/{commitSha}", CommitChanges).WithName("CommitGitChanges").RequireAuthorization("UpdateRepository");
        repo.MapGet("{url}/tree/{commitSha}", GetTree).WithName("GetGitTree").RequireAuthorization("ViewRepositoryDetails");
        repo.MapGet("{url}/file/{commitSha}/{**filePath}", GetFileContent).WithName("GetGitFileContent").RequireAuthorization("ViewRepositoryDetails");
        repo.MapPost("{url}/clone", CloneRepository).WithName("CloneGitRepository").RequireAuthorization("CreateRepository");
        repo.MapGet("{url}/branches", GetBranches).WithName("GetGitBranches").RequireAuthorization("ViewRepositoryDetails");
    }

    public static async Task<IResult> GetCommitDiff(string url, string commitSha, ISender sender)
    {
        var query = new GetCommitDiffQuery(url, commitSha);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public record CommitChangeRequest(string Author, string Email, string Message, string Content);

    public static async Task<IResult> CommitChanges([FromBody] CommitChangeRequest request, string url, string commitSha, ISender sender)
    {
        var command = new CommitChangeCommand(url, request.Author, request.Email, request.Message, request.Content);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetTree(string url, string commitSha, ISender sender)
    {
        var query = new GetListTreeQuery(url, commitSha);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetRepositoryDetails(string url, string path, ISender sender)
    {
        var query = new GetGitReposDetailsQuery(url, path);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetRepositories(ISender sender)
    {
        var query = new GetGitRepoQuery();
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> CreateRepository([FromBody] CreateGitRepositoryRequest request, ISender sender)
    {
        var command = new CreateGitRepoCommand(request.Name, request.Description, request.Url, request.ProjectId);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> DeleteRepository(string url, ISender sender)
    {
        var command = new DeleteGitRepoCommand(url);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetFileContent(string url, string commitSha, string filePath, ISender sender)
    {
        var query = new GetFileContentQuery(url, commitSha, filePath);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }

    public record CloneRepositoryRequest(string TargetPath);

    public static async Task<IResult> CloneRepository(string url, [FromBody] CloneRepositoryRequest request, ISender sender)
    {
        var command = new CloneRepositoryCommand(url, request.TargetPath);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> GetBranches(string url, ISender sender)
    {
        var query = new GetBranchesQuery(url);
        var result = await sender.Send(query);
        return result!.ToHttpResult();
    }
}
