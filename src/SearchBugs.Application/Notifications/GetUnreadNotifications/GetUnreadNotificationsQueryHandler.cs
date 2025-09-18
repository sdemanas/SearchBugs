using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.GetUnreadNotifications;

public sealed class GetUnreadNotificationsQueryHandler : IQueryHandler<GetUnreadNotificationsQuery, List<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<NotificationResponse>>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);

        var notificationResponses = notifications.Select(n => new NotificationResponse(
            n.Id.Value.ToString(),
            n.Type,
            n.Message,
            null, // Data field can be added to domain model if needed
            n.BugId?.Value.ToString(),
            n.IsRead,
            n.CreatedOnUtc)).ToList();

        return Result.Success(notificationResponses);
    }
}
