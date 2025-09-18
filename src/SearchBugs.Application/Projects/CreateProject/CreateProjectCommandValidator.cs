using FluentValidation;

namespace SearchBugs.Application.Projects.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9\s]+$")
            .WithMessage("Project name is required, must be less than 100 characters and can only contain alphanumeric characters and spaces.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Project name must be less than 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Project description is required and must be less than 500 characters.");
    }
}
