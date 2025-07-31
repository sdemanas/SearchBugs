using FluentValidation;
using SearchBugs.Application.BugTracking;
using SearchBugs.Application.BugTracking.GetBugById;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Extensions;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.Update;

public record UpdateBugCommand(
    Guid BugId,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Severity,
    Guid? AssigneeId) : ICommand<BugDto>;

public class UpdateBugCommandHandler : ICommandHandler<UpdateBugCommand, BugDto>
{
    private readonly IBugRepository _bugRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBugCommandHandler(
        IBugRepository bugRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bugRepository = bugRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BugDto>> Handle(UpdateBugCommand command, CancellationToken cancellationToken)
    {
        var bugId = new BugId(command.BugId);
        var bugResult = await _bugRepository.GetByIdAsync(bugId, cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<BugDto>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {command.BugId} not found"));
        }

        var bug = bugResult.Value;

        // Validate status
        var bugStatus = BugStatus.FromName(command.Status);
        if (bugStatus is null)
        {
            return Result.Failure<BugDto>(BugValidationErrors.InvalidBugStatus);
        }

        // Validate priority
        var bugPriority = BugPriority.FromName(command.Priority);
        if (bugPriority is null)
        {
            return Result.Failure<BugDto>(BugValidationErrors.InvalidBugPriority);
        }

        // Validate severity
        var bugSeverity = BugSeverity.FromName(command.Severity);
        if (bugSeverity is null)
        {
            return Result.Failure<BugDto>(BugValidationErrors.SeverityIsRequired);
        }

        // Update bug properties
        var updateResult = bug.Update(
            command.Title,
            command.Description,
            bugStatus,
            bugPriority,
            bugSeverity.Name,
            command.AssigneeId.HasValue ? new UserId(command.AssigneeId.Value) : null);

        if (updateResult.IsFailure)
        {
            return Result.Failure<BugDto>(updateResult.Error);
        }

        // Add to history
        bug.AddBugHistory(BugHistory.Create(
            bug.Id,
            _currentUserService.UserId,
            "Bug updated",
            "Bug details updated",
            $"Updated by {_currentUserService.UserId}"));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BugDto(
            bug.Id.Value,
            bug.Title,
            bug.Description,
            bug.Status.Name,
            bug.Priority.Name,
            bug.Severity,
            bug.ProjectId.Value,
            bug.AssigneeId.Value,
            bug.ReporterId.Value,
            bug.CreatedOnUtc,
            bug.ModifiedOnUtc));
    }
}

internal sealed class UpdateBugCommandValidator : AbstractValidator<UpdateBugCommand>
{
    public UpdateBugCommandValidator()
    {
        RuleFor(x => x.BugId)
            .NotEmpty()
            .WithError(new NotFoundError("Bug.NotFound", "The bug ID is required."));

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithError(BugValidationErrors.TitleIsRequired)
            .MaximumLength(200)
            .WithError(BugValidationErrors.TitleMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithError(BugValidationErrors.DescriptionIsRequired)
            .MaximumLength(2000)
            .WithError(BugValidationErrors.DescriptionMaxLength);

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithError(BugValidationErrors.InvalidBugStatus)
            .MaximumLength(50);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .WithError(BugValidationErrors.PriorityIsRequired)
            .MaximumLength(50);

        RuleFor(x => x.Severity)
            .NotEmpty()
            .WithError(BugValidationErrors.SeverityIsRequired)
            .MaximumLength(50);
    }
} 