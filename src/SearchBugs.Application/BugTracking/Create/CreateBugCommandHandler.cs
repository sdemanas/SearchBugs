
using SearchBugs.Application.BugTracking.GetBugs;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Data;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.Create;

public class CreateBugCommandHandler : ICommandHandler<CreateBugCommand, BugsResponse>
{
    private readonly IBugRepository _bugRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISqlQueryExecutor _sqlQueryExecutor;

    public CreateBugCommandHandler(IBugRepository bugRepository, IUnitOfWork unitOfWork, ISqlQueryExecutor sqlQueryExecutor)
    {
        _bugRepository = bugRepository;
        _unitOfWork = unitOfWork;
        _sqlQueryExecutor = sqlQueryExecutor;
    }

    public async Task<Result<BugsResponse>> Handle(CreateBugCommand request, CancellationToken cancellationToken)
    {
        var bugStatus = BugStatus.FromName(request.Status);
        var bugPriority = BugPriority.FromName(request.Priority);
        var bugSeverity = BugSeverity.FromName(request.Severity);

        if (bugStatus is null)
            return Result.Failure<BugsResponse>(BugValidationErrors.InvalidBugStatus);
        if (bugPriority is null)
            return Result.Failure<BugsResponse>(BugValidationErrors.InvalidBugPriority);
        if (bugSeverity is null)
            return Result.Failure<BugsResponse>(BugValidationErrors.InvalidBugSeverity);

        var bug = Bug.Create(
            request.Title,
            request.Description,
            bugStatus.Id,
            bugPriority.Id,
            bugSeverity.Name,
            new ProjectId(request.ProjectId),
            new UserId(request.AssigneeId),
            new UserId(request.ReporterId));

        if (bug.IsFailure)
        {
            return Result.Failure<BugsResponse>(bug.Error);
        }

        await _bugRepository.Add(bug.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch the created bug with all related data
        var createdBug = await _sqlQueryExecutor.FirstOrDefaultAsync<BugsResponse>(@"
            SELECT 
                b.id as Id,
                b.title as Title,
                b.description as Description,
                s.name as Status,
                pr.name as Priority,
                b.severity as Severity,
                p.name as ProjectName,
                CONCAT(assignee.name_first_name,' ',assignee.name_last_name) as Assignee,
                CONCAT(reporter.name_first_name,' ',reporter.name_last_name) as Reporter,
                b.created_on_utc as CreatedOnUtc,
                b.modified_on_utc as UpdatedOnUtc
            FROM bug b
            JOIN bug_status s ON b.status_id = s.id
            JOIN bug_priority pr ON b.priority_id = pr.id
            JOIN project p ON b.project_id = p.id
            LEFT JOIN ""user"" assignee ON b.assignee_id = assignee.id
            LEFT JOIN ""user"" reporter ON b.reporter_id = reporter.id
            WHERE b.id = @bugId", new { bugId = bug.Value.Id.Value });

        if (createdBug == null)
        {
            return Result.Failure<BugsResponse>(new Shared.Errors.Error("Bug.NotFound", "Bug was created but could not be retrieved"));
        }

        return Result.Success(createdBug);
    }
}
