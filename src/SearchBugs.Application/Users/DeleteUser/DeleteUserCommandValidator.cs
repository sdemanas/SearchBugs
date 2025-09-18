using FluentValidation;

namespace SearchBugs.Application.Users.DeleteUser;

internal sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required")
            .Must(x => x != Guid.Empty)
            .WithMessage("User ID cannot be empty");
    }
}
