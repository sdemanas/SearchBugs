using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs.Events;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;

namespace SearchBugs.Application.Bugs.EventHandlers;

internal sealed class BugUpdatedDomainEventHandler : IDomainEventHandler<BugUpdatedDomainEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public BugUpdatedDomainEventHandler(
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BugUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Create and persist notification
        var domainNotification = Notification.Create(
            notification.AssigneeId,
            "bug_updated",
            $"Bug '{notification.Title}' has been updated.",
            notification.BugId,
            false);

        // Send real-time notification via SignalR
        await _notificationService.SendBugNotificationAsync(
            notification.AssigneeId.Value.ToString(),
            domainNotification);
    }
}
