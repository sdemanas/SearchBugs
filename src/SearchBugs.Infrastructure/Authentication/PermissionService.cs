using Microsoft.EntityFrameworkCore;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using SearchBugs.Persistence;

namespace SearchBugs.Infrastructure.Authentication;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _dbContext;

    public PermissionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        // TODO: Remove this temporary bypass - giving all permissions to all authenticated users for testing
        return new HashSet<string>
        {
            "ListAllUsers", "ViewUserDetails", "CreateUser", "UpdateUser", "DeleteUser", "ChangeUserPassword",
            "ListAllProjects", "ViewProjectDetails", "CreateProject", "UpdateProject", "DeleteProject",
            "ListAllBugs", "ViewBugDetails", "CreateBug", "UpdateBug", "DeleteBug", "AddCommentToBug", "ViewBugComments",
            "ViewNotification", "DeleteNotification", "MarkNotificationAsRead",
            "ListAllRepositories", "ViewRepositoryDetails", "CreateRepository", "UpdateRepository", "DeleteRepository",
            "AddAttachmentToBug", "ViewBugAttachments", "ViewBugHistory", "TrackTimeSpentOnBug", "ViewTimeSpentOnBug",
            "AddCustomFieldToBug", "ViewCustomFieldOnBug", "LinkBugToRepository", "ViewBugRepository"
        };
    }
}
