using FluentValidation;
using Shared.Extensions;

namespace SearchBugs.Application.BugTracking.Create;

internal sealed class CreateBugCommandValidator : AbstractValidator<CreateBugCommand>
{
    public CreateBugCommandValidator()
    {
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

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithError(BugValidationErrors.ProjectIdIsRequired);

        RuleFor(x => x.AssigneeId)
            .NotEmpty()
            .WithError(BugValidationErrors.AssigneeIdIsRequired);

        RuleFor(x => x.ReporterId)
            .NotEmpty()
            .WithError(BugValidationErrors.ReporterIdIsRequired);
    }
}
