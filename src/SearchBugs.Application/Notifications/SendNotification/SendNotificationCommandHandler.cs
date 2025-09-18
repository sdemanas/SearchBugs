using SearchBugs.Application.Common.Interfaces;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Notifications.SendNotification;

public sealed class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand>
{
    private readonly INotificationService _notificationService;

    public SendNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var result = await _notificationService.SendNotificationToUserAsync(
            request.UserId,
            request.Type,
            request.Message,
            request.Data);

        return result;
    }
}
