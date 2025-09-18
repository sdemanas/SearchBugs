using FluentValidation;
using Microsoft.AspNetCore.Http;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Extensions;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.Attachments;

public record AddAttachmentCommand(Guid BugId, IFormFile File) : ICommand<AttachmentDto>;

public class AddAttachmentCommandHandler : ICommandHandler<AddAttachmentCommand, AttachmentDto>
{
    private readonly IBugRepository _bugRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddAttachmentCommandHandler(
        IBugRepository bugRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bugRepository = bugRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AttachmentDto>> Handle(AddAttachmentCommand command, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(command.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<AttachmentDto>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {command.BugId} not found"));
        }

        var bug = bugResult.Value;

        // Read file content
        using var stream = command.File.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var content = ms.ToArray();

        // Create attachment
        var attachment = Attachment.Create(
            command.File.FileName,
            command.File.ContentType,
            content,
            bug.Id);

        bug.AddAttachment(attachment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AttachmentDto.FromAttachment(attachment));
    }
}

internal sealed class AddAttachmentCommandValidator : AbstractValidator<AddAttachmentCommand>
{
    public AddAttachmentCommandValidator()
    {
        RuleFor(x => x.BugId)
            .NotEmpty()
            .WithError(new NotFoundError("Bug.NotFound", "The bug ID is required."));

        RuleFor(x => x.File)
            .NotNull()
            .WithError(BugValidationErrors.FileRequired)
            .Must(file => file.Length > 0)
            .WithError(BugValidationErrors.EmptyFile)
            .Must(file => file.Length <= 10 * 1024 * 1024) // 10MB max
            .WithError(BugValidationErrors.FileTooLarge);

        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .WithError(BugValidationErrors.FileNameRequired)
            .MaximumLength(255)
            .WithError(BugValidationErrors.FileNameTooLong)
            .Must(fileName => !string.IsNullOrEmpty(Path.GetExtension(fileName)))
            .WithError(BugValidationErrors.NoExtension);
    }
}
