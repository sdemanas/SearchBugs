using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.GetBugById;

public record GetBugByIdQuery(Guid BugId) : IQuery<BugDto>;

public class GetBugByIdQueryHandler : IQueryHandler<GetBugByIdQuery, BugDto>
{
    private readonly IBugRepository _bugRepository;

    public GetBugByIdQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<BugDto>> Handle(GetBugByIdQuery request, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(request.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<BugDto>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {request.BugId} not found"));
        }

        var bug = bugResult.Value;

        // Use default values if navigation properties are null
        string statusName = bug.Status?.Name ?? "Unknown Status";
        string priorityName = bug.Priority?.Name ?? "Unknown Priority";

        return Result.Success(new BugDto(
            bug.Id.Value,
            bug.Title,
            bug.Description,
            statusName,
            priorityName,
            bug.Severity,
            bug.ProjectId.Value,
            bug.AssigneeId?.Value,
            bug.ReporterId.Value,
            bug.CreatedOnUtc,
            bug.ModifiedOnUtc));
    }
}

public record BugDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Severity,
    Guid ProjectId,
    Guid? AssigneeId,
    Guid ReporterId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);