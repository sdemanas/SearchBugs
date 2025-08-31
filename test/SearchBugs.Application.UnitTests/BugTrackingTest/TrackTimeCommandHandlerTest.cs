using Moq;
using SearchBugs.Application.BugTracking.TimeTracking;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.BugTrackingTest;

public class TrackTimeCommandHandlerTest
{
    private readonly Mock<IBugRepository> _bugRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly TrackTimeCommandHandler _sut;

    public TrackTimeCommandHandlerTest()
    {
        _bugRepository = new();
        _currentUserService = new();
        _unitOfWork = new();
        _sut = new TrackTimeCommandHandler(_bugRepository.Object, _currentUserService.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenBugNotFound_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new TrackTimeCommand(bugId, TimeSpan.FromHours(2), "Working on bug fix");

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Bug>(new NotFoundError("Bug.NotFound", "Bug not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Bug.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldTrackTimeSuccessfully()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var timeSpent = TimeSpan.FromHours(3);
        var description = "Fixed authentication issue";
        var command = new TrackTimeCommand(bugId, timeSpent, description);

        var bug = Bug.Create(
            "Authentication Bug",
            "User cannot login",
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
        Assert.NotNull(result.Value);
        Assert.Equal(timeSpent, result.Value.Duration);
        Assert.Equal(userId, result.Value.UserId);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTrackingTime_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new TrackTimeCommand(bugId, TimeSpan.FromMinutes(30), "Code review");

        var bug = Bug.Create(
            "Review Bug",
            "Code needs review",
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
        _bugRepository.Verify(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()), Times.Once);
        _currentUserService.Verify(x => x.UserId, Times.AtLeastOnce);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReturnTimeEntryDto()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var duration = TimeSpan.FromHours(1.5);
        var command = new TrackTimeCommand(bugId, duration, "Working on implementation");

        var bug = Bug.Create(
            "Implementation Bug",
            "Feature implementation",
            BugStatus.InProgress.Id,
            BugPriority.Medium.Id,
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
        Assert.NotNull(result.Value);
        Assert.IsType<TimeEntryDto>(result.Value);
        Assert.Equal(duration, result.Value.Duration);
        Assert.Equal(userId, result.Value.UserId);
        Assert.True(result.Value.Id != Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserServiceCalled_ShouldUseCorrectUserId()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var specificUserId = Guid.NewGuid();
        var command = new TrackTimeCommand(bugId, TimeSpan.FromMinutes(45), "Testing user assignment");

        var bug = Bug.Create(
            "User Assignment Bug",
            "Testing user context",
            BugStatus.Open.Id,
            BugPriority.Low.Id,
            BugSeverity.Low.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(specificUserId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(specificUserId, result.Value.UserId);
        _currentUserService.Verify(x => x.UserId, Times.AtLeastOnce);
    }
}
