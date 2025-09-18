using Moq;
using SearchBugs.Application.BugTracking;
using SearchBugs.Application.BugTracking.Update;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.BugTrackingTest;

public class UpdateBugCommandHandlerTest
{
    private readonly Mock<IBugRepository> _bugRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly UpdateBugCommandHandler _sut;

    public UpdateBugCommandHandlerTest()
    {
        _bugRepository = new();
        _currentUserService = new();
        _unitOfWork = new();
        _sut = new UpdateBugCommandHandler(_bugRepository.Object, _currentUserService.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenBugNotFound_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new UpdateBugCommand(bugId, "Title", "Description", "Open", "High", "High", Guid.NewGuid());

        _bugRepository.Setup(x => x.GetByIdAsync(It.IsAny<BugId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Bug>(new NotFoundError("Bug.NotFound", "Bug not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Bug.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenStatusIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new UpdateBugCommand(bugId, "Title", "Description", "InvalidStatus", "High", "High", Guid.NewGuid());
        var bug = CreateValidBug();

        _bugRepository.Setup(x => x.GetByIdAsync(It.IsAny<BugId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(BugValidationErrors.InvalidBugStatus, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPriorityIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new UpdateBugCommand(bugId, "Title", "Description", "Open", "InvalidPriority", "High", Guid.NewGuid());
        var bug = CreateValidBug();

        _bugRepository.Setup(x => x.GetByIdAsync(It.IsAny<BugId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(BugValidationErrors.InvalidBugPriority, result.Error);
    }

    [Fact]
    public async Task Handle_WhenSeverityIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new UpdateBugCommand(bugId, "Title", "Description", "Open", "High", "InvalidSeverity", Guid.NewGuid());
        var bug = CreateValidBug();

        _bugRepository.Setup(x => x.GetByIdAsync(It.IsAny<BugId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(BugValidationErrors.SeverityIsRequired, result.Error);
    }

    [Fact]
    public async Task Handle_WhenBugHistoryIsAdded_ShouldSaveCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateBugCommand(
            BugId: Guid.NewGuid(),
            Title: "Updated Title",
            Description: "Updated Description",
            Status: BugStatus.InProgress.Name,
            Priority: BugPriority.Medium.Name,
            Severity: BugSeverity.Medium.Name,
            AssigneeId: Guid.NewGuid()
        );

        var existingBug = Bug.Create(
            "Original Title",
            "Original Description",
            BugStatus.Open.Id,
            BugPriority.Low.Id,
            BugSeverity.Low.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())
        ).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(command.BugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(existingBug));

        _currentUserService.Setup(x => x.UserId).Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAllDataIsValid_ShouldAddBugHistory()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var command = new UpdateBugCommand(bugId, "Updated Title", "Updated Description", "Open", "High", "High", assigneeId);
        var bug = CreateValidBug();
        var currentUserId = new UserId(Guid.NewGuid());

        _bugRepository.Setup(x => x.GetByIdAsync(It.IsAny<BugId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(currentUserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // Verify that AddBugHistory was called (bug history collection should have entries)
        Assert.NotEmpty(bug.BugHistories);
    }

    private static Bug CreateValidBug()
    {
        return Bug.Create(
            "Original Title",
            "Original Description",
            BugStatus.Open.Id,
            BugPriority.Medium.Id,
            BugSeverity.Medium.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;
    }
}
