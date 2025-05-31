using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Bugs;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;
using FluentValidation;
using SearchBugs.Domain;
using SearchBugs.Domain.Users;
using Shared.Extensions;

namespace SearchBugs.Application.BugTracking.Delete;

public record DeleteBugCommand(Guid BugId) : ICommand;

public class DeleteBugCommandHandler : ICommandHandler<DeleteBugCommand>
{
    private readonly IBugRepository _bugRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBugCommandHandler(
        IBugRepository bugRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bugRepository = bugRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBugCommand command, CancellationToken cancellationToken)
    {
        var bugResult = await _bugRepository.GetByIdAsync(new BugId(command.BugId), cancellationToken);
        if (bugResult.IsFailure)
        {
            return Result.Failure(new NotFoundError(
                "Bug.NotFound",
                $"Bug with ID {command.BugId} not found"));
        }

        var bug = bugResult.Value;

        // Add to history before deletion
        bug.AddBugHistory(BugHistory.Create(
            bug.Id,
            _currentUserService.UserId,
            "Bug deleted",
            "Bug details",
            $"Bug deleted by {_currentUserService.UserId.Value}"));

        await _bugRepository.Remove(bug);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class DeleteBugCommandValidator : AbstractValidator<DeleteBugCommand>
{
    public DeleteBugCommandValidator()
    {
        RuleFor(x => x.BugId)
            .NotEmpty()
            .WithErrorCode("Bug.NotFound")
            .WithMessage("The bug ID is required.");
    }
} 