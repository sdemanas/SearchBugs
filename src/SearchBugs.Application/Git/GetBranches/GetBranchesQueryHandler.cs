using SearchBugs.Domain.Git;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Git.GetBranches;

internal sealed class GetBranchesQueryHandler : IQueryHandler<GetBranchesQuery, List<string>>
{
    private readonly IGitRepositoryService _gitRepositoryService;

    public GetBranchesQueryHandler(IGitRepositoryService gitRepositoryService)
    {
        _gitRepositoryService = gitRepositoryService;
    }

    public Task<Result<List<string>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var result = _gitRepositoryService.GetBranches(request.RepoUrl);
        return Task.FromResult(result.Map(branches => branches.ToList()));
    }
}
