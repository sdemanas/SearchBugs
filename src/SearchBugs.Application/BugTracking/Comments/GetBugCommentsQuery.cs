using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;
using System.Linq;

namespace SearchBugs.Application.BugTracking.Comments;

public record GetBugCommentsQuery(Guid BugId) : IQuery<IEnumerable<CommentDto>>;

public class GetBugCommentsQueryHandler : IQueryHandler<GetBugCommentsQuery, IEnumerable<CommentDto>>
{
    private readonly IBugRepository _bugRepository;

    public GetBugCommentsQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetBugCommentsQuery request, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(request.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<IEnumerable<CommentDto>>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {request.BugId} not found"));
        }

        var bug = bugResult.Value;
        var comments = bug.Comments.Select(comment => CommentDto.FromComment(comment));
        return Result.Success(comments);
    }
} 