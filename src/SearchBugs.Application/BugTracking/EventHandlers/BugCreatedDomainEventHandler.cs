using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs.Events;
using SearchBugs.Domain.Notifications;
using Shared.Messaging;

namespace SearchBugs.Application.BugTracking.EventHandlers;

internal sealed class BugCreatedDomainEventHandler : IDomainEventHandler<BugCreatedDomainEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BugCreatedDomainEventHandler(
        INotificationService notificationService,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _notificationRepository = notificationRepository;
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

        // Persist notification to database
        await _notificationRepository.AddAsync(domainNotification, cancellationToken);

        // Send real-time notification via SignalR
        var signalRResult = await _notificationService.SendBugNotificationAsync(
            notification.AssigneeId.Value.ToString(),
            domainNotification);

        // Log SignalR failure but don't fail the entire operation
        if (signalRResult.IsFailure)
        {
            // Consider logging this error, but don't throw
            // The notification is still persisted in the database
        }

        // Also notify the reporter
        if (notification.ReporterId != notification.AssigneeId)
        {
            var reporterNotification = Notification.Create(
                notification.ReporterId,
                "bug_created",
                $"Your reported bug '{notification.Title}' has been created and assigned.",
                notification.BugId,
                false);

            // Persist reporter notification to database
            await _notificationRepository.AddAsync(reporterNotification, cancellationToken);

            var reporterSignalRResult = await _notificationService.SendBugNotificationAsync(
                notification.ReporterId.Value.ToString(),
                reporterNotification);

            // Log SignalR failure but don't fail the entire operation
            if (reporterSignalRResult.IsFailure)
            {
                // Consider logging this error, but don't throw
                // The notification is still persisted in the database
            }
        }

        // Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
