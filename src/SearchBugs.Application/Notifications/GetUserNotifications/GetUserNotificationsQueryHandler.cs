using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.GetUserNotifications;

public sealed class GetUserNotificationsQueryHandler : IQueryHandler<GetUserNotificationsQuery, PagedNotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedNotificationResponse>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);

        // Apply filtering
        if (request.IsRead.HasValue)
        {
            notifications = notifications.Where(n => n.IsRead == request.IsRead.Value).ToList();
        }

        // Apply pagination
        var totalCount = notifications.Count;
        var pagedNotifications = notifications
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationResponse(
                n.Id.Value.ToString(),
                n.Type,
                n.Message,
                null, // Data field can be added to domain model if needed
                n.BugId?.Value.ToString(),
                n.IsRead,
                n.CreatedOnUtc))
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var result = new PagedNotificationResponse(
            pagedNotifications,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Success(result);
    }
}
