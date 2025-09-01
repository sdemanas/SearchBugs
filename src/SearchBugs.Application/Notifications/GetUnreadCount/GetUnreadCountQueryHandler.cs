using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, UnreadCountResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadCountQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UnreadCountResponse>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var count = await _notificationRepository.GetUnreadCountAsync(userId);

        return Result.Success(new UnreadCountResponse(count));
    }
}
