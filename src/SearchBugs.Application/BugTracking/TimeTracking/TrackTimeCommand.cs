using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;
using FluentValidation;
using SearchBugs.Domain;
using SearchBugs.Domain.Users;
using Shared.Extensions;
using Shared.Time;

namespace SearchBugs.Application.BugTracking.TimeTracking;

public record TrackTimeCommand(Guid BugId, TimeSpan Duration, string Description) : ICommand<TimeEntryDto>;

public class TrackTimeCommandHandler : ICommandHandler<TrackTimeCommand, TimeEntryDto>
{
    private readonly IBugRepository _bugRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public TrackTimeCommandHandler(
        IBugRepository bugRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bugRepository = bugRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TimeEntryDto>> Handle(TrackTimeCommand command, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(command.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<TimeEntryDto>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {command.BugId} not found"));
        }

        var bug = bugResult.Value;
        var timeSpent = SystemTime.UtcNow.Add(command.Duration);

        var timeEntry = Domain.Bugs.TimeTracking.Create(
            bug.Id,
            _currentUserService.UserId,
            timeSpent);

        bug.AddTimeTracking(timeEntry);

        // Add to history
        bug.AddBugHistory(BugHistory.Create(
            bug.Id,
            _currentUserService.UserId,
            "Time Tracking",
            "Time spent",
            $"Added {command.Duration.TotalHours:F1} hours: {command.Description}"));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TimeEntryDto.FromTimeEntry(timeEntry, command.Duration));
    }
}

internal sealed class TrackTimeCommandValidator : AbstractValidator<TrackTimeCommand>
{
    public TrackTimeCommandValidator()
    {
        RuleFor(x => x.BugId)
            .NotEmpty()
            .WithErrorCode("Bug.NotFound")
            .WithMessage("The bug ID is required.");

        RuleFor(x => x.Duration)
            .NotEmpty()
            .WithErrorCode("TimeTracking.DurationRequired")
            .WithMessage("Duration is required.")
            .GreaterThan(TimeSpan.Zero)
            .WithErrorCode("TimeTracking.InvalidDuration")
            .WithMessage("Duration must be greater than zero.")
            .LessThanOrEqualTo(TimeSpan.FromHours(24))
            .WithErrorCode("TimeTracking.DurationTooLong")
            .WithMessage("Duration must be less than or equal to 24 hours.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithErrorCode("TimeTracking.DescriptionRequired")
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithErrorCode("TimeTracking.DescriptionTooLong")
            .WithMessage("Description must be less than 500 characters.");
    }
}

public record TimeEntryDto(
    Guid Id,
    TimeSpan Duration,
    Guid UserId,
    DateTime LoggedAt)
{
    public static TimeEntryDto FromTimeEntry(Domain.Bugs.TimeTracking entry, TimeSpan duration) => new(
        entry.Id.Value,
        duration,
        entry.UserId.Value,
        entry.LoggedAt);
} 