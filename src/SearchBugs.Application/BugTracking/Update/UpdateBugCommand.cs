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

        // Capture original values before changes
        var originalTitle = bug.Title;
        var originalDescription = bug.Description;
        var originalStatus = bug.Status?.Name;
        var originalPriority = bug.Priority?.Name;
        var originalSeverity = bug.Severity;
        var originalAssigneeId = bug.AssigneeId?.Value;

        // Get status and priority from the repository to avoid tracking conflicts
        var bugStatusResult = await _bugRepository.GetBugStatusByName(command.Status, cancellationToken);
        if (bugStatusResult.IsFailure)
        {
            return Result.Failure<BugDto>(BugValidationErrors.InvalidBugStatus);
        }
        var bugStatus = bugStatusResult.Value;

        var bugPriorityResult = await _bugRepository.GetBugPriorityByName(command.Priority, cancellationToken);
        if (bugPriorityResult.IsFailure)
        {
            return Result.Failure<BugDto>(BugValidationErrors.InvalidBugPriority);
        }
        var bugPriority = bugPriorityResult.Value;

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
            command.AssigneeId.HasValue ? new UserId(command.AssigneeId.Value) : null,
            _currentUserService.UserId);

        if (updateResult.IsFailure)
        {
            return Result.Failure<BugDto>(updateResult.Error);
        }

        // Log specific field changes
        LogFieldChange(bug, "Title", originalTitle, bug.Title);
        LogFieldChange(bug, "Description", originalDescription, bug.Description);
        LogFieldChange(bug, "Status", originalStatus, bug.Status?.Name);
        LogFieldChange(bug, "Priority", originalPriority, bug.Priority?.Name);
        LogFieldChange(bug, "Severity", originalSeverity, bug.Severity);
        LogFieldChange(bug, "Assignee",
            originalAssigneeId?.ToString() ?? "Unassigned",
            bug.AssigneeId?.Value.ToString() ?? "Unassigned");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BugDto(
            bug.Id.Value,
            bug.Title,
            bug.Description,
            bug.Status?.Name ?? string.Empty,
            bug.Priority?.Name ?? string.Empty,
            bug.Severity,
            bug.ProjectId.Value,
            bug.AssigneeId?.Value,
            bug.ReporterId.Value,
            bug.CreatedOnUtc,
            bug.ModifiedOnUtc));
    }

    private void LogFieldChange(Bug bug, string fieldName, string? oldValue, string? newValue)
    {
        if (oldValue != newValue)
        {
            bug.AddBugHistory(BugHistory.Create(
                bug.Id,
                _currentUserService.UserId,
                fieldName,
                oldValue ?? string.Empty,
                newValue ?? string.Empty));
        }
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