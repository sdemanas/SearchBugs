using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.AuditLogs.GetAuditLogs;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class AuditLogEndpoints
{
    public static void MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        var auditLogs = app.MapGroup("api/audit-logs")
            .WithTags("AuditLogs")
            .WithOpenApi();

        auditLogs.MapGet("", GetAuditLogs)
            .WithName(nameof(GetAuditLogs))
            .WithDescription("Get audit logs with optional filtering");
    }

    public static async Task<IResult> GetAuditLogs(
        ISender sender,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetAuditLogsQuery(userId, startDate, endDate, pageNumber, pageSize);
        var result = await sender.Send(query);

        return result.ToHttpResult();
    }
}
