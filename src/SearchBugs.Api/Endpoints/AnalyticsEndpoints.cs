using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SearchBugs.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public record GenerateReportRequest(
        string ReportType,
        DateTime StartDate,
        DateTime EndDate,
        Guid? ProjectId = null,
        string[]? UserIds = null,
        string? Format = "json");

    public record DashboardFilterRequest(
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        Guid? ProjectId = null,
        string[]? UserIds = null,
        string[]? Statuses = null);

    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var analytics = app.MapGroup("api/analytics")
            .WithTags("Analytics & Reporting")
            .WithOpenApi();

        // Dashboard and overview analytics - require ViewBugDetails as baseline
        analytics.MapGet("dashboard", GetDashboardAnalytics).WithName(nameof(GetDashboardAnalytics)).RequireAuthorization("ViewBugDetails");
        analytics.MapPost("dashboard", GetFilteredDashboardAnalytics).WithName(nameof(GetFilteredDashboardAnalytics)).RequireAuthorization("ViewBugDetails");

        // Bug analytics
        analytics.MapGet("bugs/summary", GetBugSummary).WithName(nameof(GetBugSummary)).RequireAuthorization("ListAllBugs");
        analytics.MapGet("bugs/trends", GetBugTrends).WithName(nameof(GetBugTrends)).RequireAuthorization("ListAllBugs");
        analytics.MapGet("bugs/by-status", GetBugsByStatus).WithName(nameof(GetBugsByStatus)).RequireAuthorization("ListAllBugs");
        analytics.MapGet("bugs/by-priority", GetBugsByPriority).WithName(nameof(GetBugsByPriority)).RequireAuthorization("ListAllBugs");
        analytics.MapGet("bugs/by-assignee", GetBugsByAssignee).WithName(nameof(GetBugsByAssignee)).RequireAuthorization("ListAllBugs");
        analytics.MapGet("bugs/resolution-time", GetBugResolutionTime).WithName(nameof(GetBugResolutionTime)).RequireAuthorization("ListAllBugs");

        // Project analytics
        analytics.MapGet("projects/summary", GetProjectSummary).WithName(nameof(GetProjectSummary)).RequireAuthorization("ListAllProjects");
        analytics.MapGet("projects/{projectId:guid}/health", GetProjectHealth).WithName(nameof(GetProjectHealth)).RequireAuthorization("ViewProjectDetails");
        analytics.MapGet("projects/{projectId:guid}/velocity", GetProjectVelocity).WithName(nameof(GetProjectVelocity)).RequireAuthorization("ViewProjectDetails");

        // User analytics
        analytics.MapGet("users/productivity", GetUserProductivity).WithName(nameof(GetUserProductivity)).RequireAuthorization("ListAllUsers");
        analytics.MapGet("users/{userId:guid}/activity", GetUserActivity).WithName("GetUserActivityAnalytics").RequireAuthorization("ViewUserDetails");
        analytics.MapGet("users/workload", GetUserWorkload).WithName(nameof(GetUserWorkload)).RequireAuthorization("ListAllUsers");

        // Time tracking analytics
        analytics.MapGet("time-tracking/summary", GetTimeTrackingSummary).WithName(nameof(GetTimeTrackingSummary)).RequireAuthorization("ViewTimeSpentOnBug");
        analytics.MapGet("time-tracking/by-project", GetTimeTrackingByProject).WithName(nameof(GetTimeTrackingByProject)).RequireAuthorization("ViewTimeSpentOnBug");
        analytics.MapGet("time-tracking/by-user", GetTimeTrackingByUser).WithName(nameof(GetTimeTrackingByUser)).RequireAuthorization("ViewTimeSpentOnBug");

        // Report generation - require higher permissions
        analytics.MapPost("reports/generate", GenerateReport).WithName(nameof(GenerateReport)).RequireAuthorization("ViewBugDetails");
        analytics.MapGet("reports", GetAvailableReports).WithName(nameof(GetAvailableReports)).RequireAuthorization("ViewBugDetails");
        analytics.MapGet("reports/{reportId:guid}", GetReportStatus).WithName(nameof(GetReportStatus)).RequireAuthorization("ViewBugDetails");
        analytics.MapGet("reports/{reportId:guid}/download", DownloadReport).WithName(nameof(DownloadReport)).RequireAuthorization("ViewBugDetails");

        // System health and metrics - admin only
        analytics.MapGet("system/health", GetSystemHealth).WithName(nameof(GetSystemHealth)).RequireAuthorization("ListAllUsers");
        analytics.MapGet("system/performance", GetSystemPerformance).WithName(nameof(GetSystemPerformance)).RequireAuthorization("ListAllUsers");
    }

    // Dashboard Analytics
    public static Task<IResult> GetDashboardAnalytics(ISender sender)
    {
        // Note: This would require implementing GetDashboardAnalyticsQuery in the Application layer
        return Task.FromResult(Results.Ok("Dashboard analytics data"));
    }

    public static Task<IResult> GetFilteredDashboardAnalytics(
        [FromBody] DashboardFilterRequest request,
        ISender sender)
    {
        // Note: This would require implementing GetFilteredDashboardAnalyticsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Filtered dashboard analytics for date range {request.StartDate} to {request.EndDate}"));
    }

    // Bug Analytics
    public static Task<IResult> GetBugSummary(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? projectId = null)
    {
        // Note: This would require implementing GetBugSummaryQuery in the Application layer
        return Task.FromResult(Results.Ok($"Bug summary for project {projectId}"));
    }

    public static Task<IResult> GetBugTrends(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string period = "weekly")
    {
        // Note: This would require implementing GetBugTrendsQuery in the Application layer
        return Task.FromResult(Results.Ok($"Bug trends for {period} periods"));
    }

    public static Task<IResult> GetBugsByStatus(
        ISender sender,
        [FromQuery] Guid? projectId = null)
    {
        // Note: This would require implementing GetBugsByStatusQuery in the Application layer
        return Task.FromResult(Results.Ok($"Bugs grouped by status for project {projectId}"));
    }

    public static Task<IResult> GetBugsByPriority(
        ISender sender,
        [FromQuery] Guid? projectId = null)
    {
        // Note: This would require implementing GetBugsByPriorityQuery in the Application layer
        return Task.FromResult(Results.Ok($"Bugs grouped by priority for project {projectId}"));
    }

    public static Task<IResult> GetBugsByAssignee(
        ISender sender,
        [FromQuery] Guid? projectId = null)
    {
        // Note: This would require implementing GetBugsByAssigneeQuery in the Application layer
        return Task.FromResult(Results.Ok($"Bugs grouped by assignee for project {projectId}"));
    }

    public static Task<IResult> GetBugResolutionTime(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Note: This would require implementing GetBugResolutionTimeQuery in the Application layer
        return Task.FromResult(Results.Ok("Bug resolution time analytics"));
    }

    // Project Analytics
    public static Task<IResult> GetProjectSummary(ISender sender)
    {
        // Note: This would require implementing GetProjectSummaryQuery in the Application layer
        return Task.FromResult(Results.Ok("Project summary analytics"));
    }

    public static Task<IResult> GetProjectHealth(Guid projectId, ISender sender)
    {
        // Note: This would require implementing GetProjectHealthQuery in the Application layer
        return Task.FromResult(Results.Ok($"Health metrics for project {projectId}"));
    }

    public static Task<IResult> GetProjectVelocity(
        Guid projectId,
        ISender sender,
        [FromQuery] int weeks = 12)
    {
        // Note: This would require implementing GetProjectVelocityQuery in the Application layer
        return Task.FromResult(Results.Ok($"Velocity metrics for project {projectId} over {weeks} weeks"));
    }

    // User Analytics
    public static Task<IResult> GetUserProductivity(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Note: This would require implementing GetUserProductivityQuery in the Application layer
        return Task.FromResult(Results.Ok("User productivity analytics"));
    }

    public static Task<IResult> GetUserActivity(
        Guid userId,
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Note: This would require implementing GetUserActivityQuery in the Application layer
        return Task.FromResult(Results.Ok($"Activity analytics for user {userId}"));
    }

    public static Task<IResult> GetUserWorkload(ISender sender)
    {
        // Note: This would require implementing GetUserWorkloadQuery in the Application layer
        return Task.FromResult(Results.Ok("User workload distribution"));
    }

    // Time Tracking Analytics
    public static Task<IResult> GetTimeTrackingSummary(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Note: This would require implementing GetTimeTrackingSummaryQuery in the Application layer
        return Task.FromResult(Results.Ok("Time tracking summary"));
    }

    public static Task<IResult> GetTimeTrackingByProject(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Note: This would require implementing GetTimeTrackingByProjectQuery in the Application layer
        return Task.FromResult(Results.Ok("Time tracking grouped by project"));
    }

    public static Task<IResult> GetTimeTrackingByUser(
        ISender sender,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Note: This would require implementing GetTimeTrackingByUserQuery in the Application layer
        return Task.FromResult(Results.Ok("Time tracking grouped by user"));
    }

    // Report Generation
    public static Task<IResult> GenerateReport(
        [FromBody] GenerateReportRequest request,
        ISender sender)
    {
        // Note: This would require implementing GenerateReportCommand in the Application layer
        return Task.FromResult(Results.Ok($"Generate {request.ReportType} report from {request.StartDate} to {request.EndDate}"));
    }

    public static Task<IResult> GetAvailableReports(ISender sender)
    {
        // Note: This would require implementing GetAvailableReportsQuery in the Application layer
        return Task.FromResult(Results.Ok("List of available report types"));
    }

    public static Task<IResult> GetReportStatus(Guid reportId, ISender sender)
    {
        // Note: This would require implementing GetReportStatusQuery in the Application layer
        return Task.FromResult(Results.Ok($"Status of report {reportId}"));
    }

    public static Task<IResult> DownloadReport(Guid reportId, ISender sender)
    {
        // Note: This would require implementing DownloadReportQuery in the Application layer
        return Task.FromResult(Results.Ok($"Download report {reportId}"));
    }

    // System Analytics
    public static Task<IResult> GetSystemHealth(ISender sender)
    {
        // Note: This would require implementing GetSystemHealthQuery in the Application layer
        return Task.FromResult(Results.Ok("System health metrics"));
    }

    public static Task<IResult> GetSystemPerformance(ISender sender)
    {
        // Note: This would require implementing GetSystemPerformanceQuery in the Application layer
        return Task.FromResult(Results.Ok("System performance metrics"));
    }
}
