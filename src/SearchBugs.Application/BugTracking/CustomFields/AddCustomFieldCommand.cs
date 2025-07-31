using Shared.Messaging;
using SearchBugs.Application.Common.Interfaces;
using FluentValidation;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Extensions;
using Shared.Results;
using SearchBugs.Domain;
using SearchBugs.Domain.Projects;

namespace SearchBugs.Application.BugTracking.CustomFields;

public record AddCustomFieldCommand(Guid BugId, string Name, string Value) : ICommand<CustomFieldDto>;

public class AddCustomFieldCommandHandler : ICommandHandler<AddCustomFieldCommand, CustomFieldDto>
{
    private readonly IBugRepository _bugRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddCustomFieldCommandHandler(
        IBugRepository bugRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bugRepository = bugRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomFieldDto>> Handle(AddCustomFieldCommand command, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(command.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<CustomFieldDto>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {command.BugId} not found"));
        }

        var bug = bugResult.Value;

        // Create the custom field first
        var customField = CustomField.Create(
            command.Name,
            "text", // Default field type for now
            bug.ProjectId);

        // Create the bug custom field link
        var bugCustomField = BugCustomField.Create(
            bug.Id,
            customField.Id,
            command.Value);

        bug.AddBugCustomField(bugCustomField);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CustomFieldDto.FromCustomField(customField, command.Value));
    }
}

internal sealed class AddCustomFieldCommandValidator : AbstractValidator<AddCustomFieldCommand>
{
    public AddCustomFieldCommandValidator()
    {
        RuleFor(x => x.BugId)
            .NotEmpty()
            .WithError(new NotFoundError("Bug.NotFound", "The bug ID is required."));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithError(new Error("CustomField.NameIsRequired", "The custom field name is required."))
            .MaximumLength(200)
            .WithError(new Error("CustomField.NameMaxLength", "The custom field name must not exceed 200 characters."))
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithError(new Error("CustomField.InvalidName", "Name can only contain letters, numbers, underscores and hyphens"));

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithError(new Error("CustomField.ValueIsRequired", "The custom field value is required."))
            .MaximumLength(2000)
            .WithError(new Error("CustomField.ValueMaxLength", "The custom field value must not exceed 2000 characters."));
    }
} 