using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;
namespace SearchBugs.Application.BugTracking.History;

public record GetBugHistoryQuery(Guid BugId) : IQuery<IEnumerable<HistoryEntryDto>>;

public class GetBugHistoryQueryHandler : IQueryHandler<GetBugHistoryQuery, IEnumerable<HistoryEntryDto>>
{
    private readonly IBugRepository _bugRepository;

    public GetBugHistoryQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<IEnumerable<HistoryEntryDto>>> Handle(GetBugHistoryQuery request, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(request.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<IEnumerable<HistoryEntryDto>>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {request.BugId} not found"));
        }

        var bug = bugResult.Value;
        var historyEntries = bug.BugHistories.Select(entry => HistoryEntryDto.FromHistoryEntry(entry));
        return Result.Success(historyEntries);
    }
}

public record HistoryEntryDto(
    Guid Id,
    string FieldChanged,
    string OldValue,
    string NewValue,
    Guid ChangedById,
    DateTime ChangedAtUtc)
{
    public static HistoryEntryDto FromHistoryEntry(BugHistory entry) => new(
        entry.Id.Value,
        entry.FieldChanged,
        entry.OldValue,
        entry.NewValue,
        entry.ChangedBy.Value,
        entry.ChangedAtUtc);
} 