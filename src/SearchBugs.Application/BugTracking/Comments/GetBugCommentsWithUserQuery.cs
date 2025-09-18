using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.Comments;

public record GetBugCommentsWithUserQuery(Guid BugId) : IQuery<IEnumerable<CommentWithUserDto>>;

public class GetBugCommentsWithUserQueryHandler : IQueryHandler<GetBugCommentsWithUserQuery, IEnumerable<CommentWithUserDto>>
{
    private readonly IBugRepository _bugRepository;

    public GetBugCommentsWithUserQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<IEnumerable<CommentWithUserDto>>> Handle(GetBugCommentsWithUserQuery request, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(request.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<IEnumerable<CommentWithUserDto>>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {request.BugId} not found"));
        }

        var bug = bugResult.Value;
        var comments = bug.Comments
            .OrderBy(c => c.CreatedOnUtc)
            .Select(comment => new CommentWithUserDto(
                comment.Id.Value,
                comment.CommentText,
                comment.UserId.Value,
                comment.CreatedOnUtc,
                comment.ModifiedOnUtc,
                comment.User != null ? $"{comment.User.Name.FirstName} {comment.User.Name.LastName}".Trim() : "Unknown User",
                comment.User?.Email?.Value));

        return Result.Success(comments);
    }
}

public record CommentWithUserDto(
    Guid Id,
    string CommentText,
    Guid UserId,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc,
    string UserName,
    string? UserEmail);
