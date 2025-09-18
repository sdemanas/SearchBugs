using FluentValidation;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.BugTracking.Comments;

public record AddCommentCommand(Guid BugId, string Content) : ICommand<CommentDto>;

public class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, CommentDto>
{
    private readonly IBugRepository _bugRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(
        IBugRepository bugRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bugRepository = bugRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CommentDto>> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(command.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure<CommentDto>(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {command.BugId} not found"));
        }

        var bug = bugResult.Value;

        var comment = Comment.Create(
            bug.Id,
            _currentUserService.UserId,
            command.Content);

        bug.AddComment(comment);

        // Add to history
        bug.AddBugHistory(BugHistory.Create(
            bug.Id,
            _currentUserService.UserId,
            "Comment",
            string.Empty,
            "Added comment"));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CommentDto.FromComment(comment));
    }
}

internal sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.BugId)
            .NotEmpty()
            .WithErrorCode("Bug.NotFound")
            .WithMessage("The bug ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithErrorCode("Comment.ContentRequired")
            .WithMessage("Comment content is required.")
            .MaximumLength(2000)
            .WithErrorCode("Comment.ContentTooLong")
            .WithMessage("Comment content must be less than 2000 characters.");
    }
}

public record CommentDto(
    Guid Id,
    string CommentText,
    Guid UserId,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc)
{
    public static CommentDto FromComment(Comment comment) => new(
        comment.Id.Value,
        comment.CommentText,
        comment.UserId.Value,
        comment.CreatedOnUtc,
        comment.ModifiedOnUtc);
}
