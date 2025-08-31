using Moq;
using SearchBugs.Application.BugTracking.Comments;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.BugTrackingTest;

public class AddCommentCommandHandlerTest
{
    private readonly Mock<IBugRepository> _bugRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly AddCommentCommandHandler _sut;

    public AddCommentCommandHandlerTest()
    {
        _bugRepository = new();
        _currentUserService = new();
        _unitOfWork = new();
        _sut = new AddCommentCommandHandler(_bugRepository.Object, _currentUserService.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenBugNotFound_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new AddCommentCommand(bugId, "This is a comment");

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Bug>(new NotFoundError("Bug.NotFound", "Bug not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Bug.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldAddCommentSuccessfully()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var commentContent = "This is a test comment";
        var command = new AddCommentCommand(bugId, commentContent);

        var bug = Bug.Create(
            "Test Bug",
            "Test Description",
            BugStatus.Open.Id,
            BugPriority.Medium.Id,
            BugSeverity.Medium.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(commentContent, result.Value.CommentText);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAddingComment_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new AddCommentCommand(bugId, "Another comment");

        var bug = Bug.Create(
            "Another Bug",
            "Another Description",
            BugStatus.InProgress.Id,
            BugPriority.High.Id,
            BugSeverity.High.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _bugRepository.Verify(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()), Times.Once);
        _currentUserService.Verify(x => x.UserId, Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCommentContentIsProvided_ShouldCreateCommentWithContent()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedContent = "Expected comment content";
        var command = new AddCommentCommand(bugId, expectedContent);

        var bug = Bug.Create(
            "Content Test Bug",
            "Content Test Description",
            BugStatus.Open.Id,
            BugPriority.Low.Id,
            BugSeverity.Low.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedContent, result.Value.CommentText);
        Assert.NotEmpty(bug.Comments);
        var addedComment = bug.Comments.First();
        Assert.Equal(expectedContent, addedComment.CommentText);
        Assert.Equal(new UserId(userId), addedComment.UserId);
        Assert.Equal(bug.Id, addedComment.BugId);
    }

    [Fact]
    public async Task Handle_WhenMultipleCommentsAdded_ShouldMaintainCommentHistory()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var firstComment = "First comment";
        var secondComment = "Second comment";

        var bug = Bug.Create(
            "Multi Comment Bug",
            "Multi Comment Description",
            BugStatus.Open.Id,
            BugPriority.Medium.Id,
            BugSeverity.Medium.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        var firstCommand = new AddCommentCommand(bugId, firstComment);
        var secondCommand = new AddCommentCommand(bugId, secondComment);

        // Act
        var firstResult = await _sut.Handle(firstCommand, CancellationToken.None);
        var secondResult = await _sut.Handle(secondCommand, CancellationToken.None);

        // Assert
        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(2, bug.Comments.Count);
        Assert.Contains(bug.Comments, c => c.CommentText == firstComment);
        Assert.Contains(bug.Comments, c => c.CommentText == secondComment);
    }
}
