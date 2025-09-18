using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.ClearAllNotifications;

public sealed class ClearAllNotificationsCommandHandler : ICommandHandler<ClearAllNotificationsCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ClearAllNotificationsCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ClearAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);

        foreach (var notification in notifications)
        {
            await _notificationRepository.Remove(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
