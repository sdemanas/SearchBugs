using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.MarkAllAsRead;

public sealed class MarkAllAsReadCommandHandler : ICommandHandler<MarkAllAsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var unreadNotifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
            await _notificationRepository.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
