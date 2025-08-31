using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs.Events;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;

namespace SearchBugs.Application.Bugs.EventHandlers;

internal sealed class BugAssignedDomainEventHandler : IDomainEventHandler<BugAssignedDomainEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public BugAssignedDomainEventHandler(
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BugAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Create and persist notification
        var domainNotification = Notification.Create(
            notification.AssigneeId,
            "bug_assigned",
            $"Bug '{notification.Title}' has been assigned to you.",
            notification.BugId,
            false);

        // Send real-time notification via SignalR
        await _notificationService.SendBugNotificationAsync(
            notification.AssigneeId.Value.ToString(),
            domainNotification);
    }
}
