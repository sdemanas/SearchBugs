using FluentValidation;
using Shared.Extensions;

namespace SearchBugs.Application.Users.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithError(UserValidationErrors.EmailIsRequired)
            .EmailAddress().WithMessage("Please enter a valid email address.");
    }
}
