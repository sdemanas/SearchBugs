using FluentValidation;

namespace SearchBugs.Application.Users.AssignRole;

internal class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty()
            .Must(x => x != Guid.Empty);
        RuleFor(x => x.Role).NotEmpty();
    }
}
