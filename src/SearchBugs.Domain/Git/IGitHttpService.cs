using Microsoft.AspNetCore.Http;

namespace SearchBugs.Domain.Git;

public interface IGitHttpService
{
    Task Handle(string repositoryName,
    string path,
        HttpContext httpContext,
        CancellationToken cancellationToken = default);

    Task CreateRepository(string repositoryName, CancellationToken cancellationToken = default);

    Task DeleteRepository(string repositoryName, CancellationToken cancellationToken = default);
}
