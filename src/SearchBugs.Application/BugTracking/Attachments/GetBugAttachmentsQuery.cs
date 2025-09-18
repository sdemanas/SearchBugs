using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.Attachments;

public record GetBugAttachmentsQuery(Guid BugId) : IQuery<IEnumerable<AttachmentDto>>;

public class GetBugAttachmentsQueryHandler : IQueryHandler<GetBugAttachmentsQuery, IEnumerable<AttachmentDto>>
{
    private readonly IBugRepository _bugRepository;

    public GetBugAttachmentsQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> Handle(GetBugAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(request.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<IEnumerable<AttachmentDto>>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {request.BugId} not found"));
        }

        var bug = bugResult.Value;
        var attachments = bug.Attachments.Select(attachment => AttachmentDto.FromAttachment(attachment));
        return Result.Success(attachments);
    }
}
