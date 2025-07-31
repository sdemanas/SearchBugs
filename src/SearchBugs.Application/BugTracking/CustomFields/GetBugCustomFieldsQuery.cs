using Shared.Messaging;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.CustomFields;

public record GetBugCustomFieldsQuery(Guid BugId) : IQuery<IEnumerable<CustomFieldDto>>;

public class GetBugCustomFieldsQueryHandler : IQueryHandler<GetBugCustomFieldsQuery, IEnumerable<CustomFieldDto>>
{
    private readonly IBugRepository _bugRepository;

    public GetBugCustomFieldsQueryHandler(IBugRepository bugRepository)
    {
        _bugRepository = bugRepository;
    }

    public async Task<Result<IEnumerable<CustomFieldDto>>> Handle(GetBugCustomFieldsQuery query, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(query.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<IEnumerable<CustomFieldDto>>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {query.BugId} not found"));
        }

        var bug = bugResult.Value;
        var customFields = bug.BugCustomFields
            .Select(bcf => CustomFieldDto.FromCustomField(bcf.CustomField, bcf.Value));

        return Result.Success(customFields);
    }
} 