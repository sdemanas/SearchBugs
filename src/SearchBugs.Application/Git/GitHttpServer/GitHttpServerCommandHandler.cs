using Microsoft.AspNetCore.Http;
using SearchBugs.Domain.Git;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Git.GitHttpServer;

internal sealed class GitHttpServerCommandHandler : ICommandHandler<GitHttpServerCommand>
{
    private readonly IGitHttpService _gitService;

    public GitHttpServerCommandHandler(IGitHttpService gitService) => _gitService = gitService;

    public async Task<Result> Handle(GitHttpServerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.HttpContext.Request.Method == "GET" &&
                command.Path.EndsWith("/info/refs"))
            {
                await _gitService.CreateRepository(command.Name, cancellationToken);
            }

            await _gitService.Handle(
                command.Name,
                command.Path,
                command.HttpContext,
                cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            command.HttpContext.Response.StatusCode = ex switch
            {
                DirectoryNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            await command.HttpContext.Response.WriteAsync(ex.Message);
            return Result.Failure(Error.ConditionNotMet);
        }
    }
}
