using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs.Events;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;

namespace SearchBugs.Application.Bugs.EventHandlers;

internal sealed class BugCreatedDomainEventHandler : IDomainEventHandler<BugCreatedDomainEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public BugCreatedDomainEventHandler(
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BugCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Create and persist notification
        var domainNotification = Notification.Create(
            notification.AssigneeId,
            "bug_created",
            $"A new bug '{notification.Title}' has been assigned to you.",
            notification.BugId,
            false);

        // Send real-time notification via SignalR
        await _notificationService.SendBugNotificationAsync(
            notification.AssigneeId.Value.ToString(),
            domainNotification);

        // Also notify the reporter
        if (notification.ReporterId != notification.AssigneeId)
        {
            var reporterNotification = Notification.Create(
                notification.ReporterId,
                "bug_created",
                $"Your reported bug '{notification.Title}' has been created and assigned.",
                notification.BugId,
                false);

            await _notificationService.SendBugNotificationAsync(
                notification.ReporterId.Value.ToString(),
                reporterNotification);
        }
    }
}
