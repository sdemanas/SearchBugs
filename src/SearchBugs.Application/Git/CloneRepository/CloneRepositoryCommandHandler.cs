using SearchBugs.Domain.Git;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Git.CloneRepository;

internal sealed class CloneRepositoryCommandHandler : ICommandHandler<CloneRepositoryCommand>
{
    private readonly IGitRepositoryService _gitRepositoryService;

    public CloneRepositoryCommandHandler(IGitRepositoryService gitRepositoryService)
    {
        _gitRepositoryService = gitRepositoryService;
    }

    public Task<Result> Handle(CloneRepositoryCommand request, CancellationToken cancellationToken)
    {
        var result = _gitRepositoryService.CloneRepository(request.SourceUrl, request.TargetPath);
        return Task.FromResult(result);
    }
}
