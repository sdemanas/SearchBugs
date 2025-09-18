using MediatR;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.AuditLogs;
using SearchBugs.Domain.Repositories;
using Shared.Results;

namespace SearchBugs.Application.Users.GetUserActivity;

internal sealed class GetUserActivityQueryHandler
    : IRequestHandler<GetUserActivityQuery, Result<GetUserActivityResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;

    public GetUserActivityQueryHandler(
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository)
    {
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<GetUserActivityResponse>> Handle(
        GetUserActivityQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // Get all user audit logs first (in a real app, you'd implement pagination at the repository level)
        var allAuditLogs = await _auditLogRepository.GetByUserIdAsync(userId.Value, cancellationToken);

        // Calculate pagination
        var totalCount = allAuditLogs.Count;
        var skip = (request.PageNumber - 1) * request.PageSize;
        var pagedAuditLogs = allAuditLogs.Skip(skip).Take(request.PageSize).ToArray();

        var activities = pagedAuditLogs.Select(MapAuditLogToActivity).ToArray();

        var hasNextPage = (request.PageNumber * request.PageSize) < totalCount;
        var hasPreviousPage = request.PageNumber > 1;

        var response = new GetUserActivityResponse(
            Activities: activities,
            TotalCount: totalCount,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            HasNextPage: hasNextPage,
            HasPreviousPage: hasPreviousPage
        );

        return Result.Success(response);
    }

    private static UserActivityItem MapAuditLogToActivity(AuditLog auditLog)
    {
        var type = GetActivityType(auditLog.RequestName);
        var action = GetActivityAction(auditLog.RequestName);
        var target = GetActivityTarget(auditLog.RequestName, auditLog.RequestData);
        var description = GetActivityDescription(auditLog.RequestName, action, target, auditLog.IsSuccess);

        return new UserActivityItem(
            Id: auditLog.Id.Value.ToString(),
            Type: type,
            Action: action,
            Target: target,
            Description: description,
            Timestamp: auditLog.CreatedOnUtc,
            Icon: GetActivityIcon(auditLog.RequestName),
            IsSuccess: auditLog.IsSuccess,
            Duration: auditLog.Duration,
            ErrorMessage: auditLog.ErrorMessage
        );
    }

    private static string GetActivityType(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Bug") => "bug",
            var name when name.Contains("Project") => "project",
            var name when name.Contains("User") && !name.Contains("Login") => "user-management",
            var name when name.Contains("Role") => "role-management",
            var name when name.Contains("Login") => "authentication",
            var name when name.Contains("Password") => "security",
            var name when name.Contains("Repository") || name.Contains("Repo") => "repository",
            var name when name.Contains("Notification") => "notification",
            _ => "system"
        };
    }

    private static string GetActivityAction(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Create") => "created",
            var name when name.Contains("Update") => "updated",
            var name when name.Contains("Delete") => "deleted",
            var name when name.Contains("Login") => "logged in",
            var name when name.Contains("Register") => "registered",
            var name when name.Contains("ChangePassword") => "changed password",
            var name when name.Contains("AssignRole") => "assigned role",
            var name when name.Contains("RemoveRole") => "removed role",
            var name when name.Contains("Clone") => "cloned",
            var name when name.Contains("Push") => "pushed to",
            var name when name.Contains("Pull") => "pulled from",
            _ => "performed action on"
        };
    }

    private static string GetActivityTarget(string requestName, string requestData)
    {
        // Try to extract meaningful target from request name
        if (requestName.Contains("Bug"))
            return "bug report";
        if (requestName.Contains("Project"))
            return "project";
        if (requestName.Contains("User") && !requestName.Contains("Login"))
            return "user account";
        if (requestName.Contains("Role"))
            return "user role";
        if (requestName.Contains("Login"))
            return "system";
        if (requestName.Contains("Password"))
            return "account password";
        if (requestName.Contains("Repository") || requestName.Contains("Repo"))
            return "repository";
        if (requestName.Contains("Notification"))
            return "notification";

        return requestName.Replace("Command", "").Replace("Query", "").ToLowerInvariant();
    }

    private static string GetActivityDescription(string requestName, string action, string target, bool isSuccess)
    {
        var statusText = isSuccess ? "successfully" : "failed to";
        var article = GetArticle(target);

        return $"{statusText} {action} {article} {target}".Trim();
    }

    private static string GetArticle(string word)
    {
        if (string.IsNullOrEmpty(word))
            return "";

        var firstChar = char.ToLower(word[0]);
        var vowels = new[] { 'a', 'e', 'i', 'o', 'u' };

        return vowels.Contains(firstChar) ? "an" : "a";
    }

    private static string GetActivityIcon(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Bug") => "Bug",
            var name when name.Contains("Project") => "FolderOpen",
            var name when name.Contains("User") && !name.Contains("Login") => "User",
            var name when name.Contains("Role") => "Shield",
            var name when name.Contains("Login") => "LogIn",
            var name when name.Contains("Register") => "UserPlus",
            var name when name.Contains("Password") => "Lock",
            var name when name.Contains("Repository") || name.Contains("Repo") => "GitBranch",
            var name when name.Contains("Notification") => "Bell",
            var name when name.Contains("Create") => "Plus",
            var name when name.Contains("Update") => "Edit",
            var name when name.Contains("Delete") => "Trash2",
            _ => "Activity"
        };
    }
}
