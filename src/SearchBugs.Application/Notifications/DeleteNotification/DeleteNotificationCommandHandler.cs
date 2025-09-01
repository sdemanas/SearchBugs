using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Notifications;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.DeleteNotification;

public sealed class DeleteNotificationCommandHandler : ICommandHandler<DeleteNotificationCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.NotificationId, out var notificationGuid))
        {
            return Result.Failure(new Error("Notification.InvalidId", "Invalid notification ID format"));
        }

        var notificationId = new NotificationId(notificationGuid);
        var userId = _currentUserService.UserId;

        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var notification = notifications.FirstOrDefault(n => n.Id == notificationId);

        if (notification == null)
        {
            return Result.Failure(new Error("Notification.NotFound", "Notification not found"));
        }

        await _notificationRepository.Remove(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
