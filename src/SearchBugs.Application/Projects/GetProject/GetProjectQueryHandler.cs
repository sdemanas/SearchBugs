using Shared.Data;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Projects.GetProject;

internal sealed class GetProjectQueryHandler : IQueryHandler<GetProjectQuery, GetProjectResponse>
{
    private readonly ISqlQueryExecutor _sqlQueryExecutor;

    public GetProjectQueryHandler(ISqlQueryExecutor sqlQueryExecutor) => _sqlQueryExecutor = sqlQueryExecutor;

    public Task<Result<GetProjectResponse>> Handle(GetProjectQuery request, CancellationToken cancellationToken) =>
        Result.Create(request)
            .Bind(async query => Result.Create(await GetProjectAsync(query.ProjectId)))
            .Map(project => project ?? throw new InvalidOperationException("Project not found"));

    private async Task<GetProjectResponse?> GetProjectAsync(Guid projectId) =>
        await _sqlQueryExecutor.FirstOrDefaultAsync<GetProjectResponse>(@"
            SELECT 
                p.id as Id,
                p.name as Name,
                p.description as Description,
                p.created_on_utc as CreatedOnUtc,
                p.modified_on_utc as UpdatedOnUtc
            FROM project p
            WHERE p.id = @ProjectId", new { ProjectId = projectId });
}
