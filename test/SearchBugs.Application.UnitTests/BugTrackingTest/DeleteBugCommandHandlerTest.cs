using Moq;
using SearchBugs.Application.BugTracking.Delete;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.BugTrackingTest;

public class DeleteBugCommandHandlerTest
{
    private readonly Mock<IBugRepository> _bugRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly DeleteBugCommandHandler _sut;

    public DeleteBugCommandHandlerTest()
    {
        _bugRepository = new();
        _currentUserService = new();
        _unitOfWork = new();
        _sut = new DeleteBugCommandHandler(_bugRepository.Object, _currentUserService.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenBugNotFound_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new DeleteBugCommand(bugId);

        _bugRepository.Setup(x => x.GetByIdAsync(It.IsAny<BugId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Bug>(new NotFoundError("Bug.NotFound", "Bug not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Bug.NotFound", result.Error.Code);
        Assert.Contains("not found", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldReturnSuccess()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new DeleteBugCommand(bugId);
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
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldCallRemoveBug()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new DeleteBugCommand(bugId);
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
        _bugRepository.Verify(x => x.Remove(bug), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldSaveChanges()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new DeleteBugCommand(bugId);
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
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldAddBugHistory()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new DeleteBugCommand(bugId);
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
            "Test Bug",
            "Test Description",
            BugStatus.Open.Id,
            BugPriority.Medium.Id,
            BugSeverity.Medium.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;
    }
}
