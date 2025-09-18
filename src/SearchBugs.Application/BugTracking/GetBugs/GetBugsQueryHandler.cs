using Shared.Data;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.GetBugs;

public sealed class GetBugsQueryHandler : IQueryHandler<GetBugsQuery, List<BugsResponse>>
{
    private readonly ISqlQueryExecutor _sqlQueryExecutor;

    public GetBugsQueryHandler(ISqlQueryExecutor sqlQueryExecutor) => _sqlQueryExecutor = sqlQueryExecutor;


    public Task<Result<List<BugsResponse>>> Handle(GetBugsQuery request, CancellationToken cancellationToken) =>
        Result.Create(request)
            .Bind(async query => Result.Create(await GetBugsByProjectIdAsync(query.ProjectId)))
            .Map(bugs => bugs.ToList());


    public async Task<IEnumerable<BugsResponse>?> GetBugsByProjectIdAsync(Guid? projectId)
    {
        if (projectId.HasValue)
        {
            // Get bugs for a specific project
            return await _sqlQueryExecutor.QueryAsync<BugsResponse>(@"
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
                WHERE b.project_id = @projectId
                ORDER BY b.created_on_utc DESC", new { projectId });
        }
        else
        {
            // Get all bugs across all projects
            return await _sqlQueryExecutor.QueryAsync<BugsResponse>(@"
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
                ORDER BY b.created_on_utc DESC");
        }
    }
}
