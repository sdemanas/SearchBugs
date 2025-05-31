using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;
using Shared.Time;

namespace SearchBugs.Application.BugTracking.TimeTracking;

public record GetBugTimeTrackingQuery(Guid BugId) : IQuery<IEnumerable<TimeEntryDto>>;

public class GetBugTimeTrackingQueryHandler : IQueryHandler<GetBugTimeTrackingQuery, IEnumerable<TimeEntryDto>>
{
    private readonly IBugRepository _bugRepository;

    public GetBugTimeTrackingQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> Handle(GetBugTimeTrackingQuery request, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(request.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<IEnumerable<TimeEntryDto>>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {request.BugId} not found"));
        }

        var bug = bugResult.Value;
        var timeEntries = bug.TimeTracking.Select(entry => 
        {
            var duration = entry.TimeSpent.HasValue 
                ? entry.TimeSpent.Value - entry.LoggedAt 
                : TimeSpan.Zero;
            return TimeEntryDto.FromTimeEntry(entry, duration);
        });
        return Result.Success(timeEntries);
    }
} 