using Moq;
using SearchBugs.Application.BugTracking.CustomFields;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.BugTrackingTest;

public class AddCustomFieldCommandHandlerTest
{
    private readonly Mock<IBugRepository> _bugRepository;
    private readonly Mock<IProjectRepository> _projectRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly AddCustomFieldCommandHandler _sut;

    public AddCustomFieldCommandHandlerTest()
    {
        _bugRepository = new();
        _projectRepository = new();
        _currentUserService = new();
        _unitOfWork = new();
        _sut = new AddCustomFieldCommandHandler(_bugRepository.Object, _projectRepository.Object, _currentUserService.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenBugNotFound_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var command = new AddCustomFieldCommand(bugId, "Priority Level", "High");

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Bug>(new NotFoundError("Bug.NotFound", "Bug not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Bug.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldAddCustomFieldSuccessfully()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var projectId = new ProjectId(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var fieldName = "Environment";
        var fieldValue = "Production";
        var command = new AddCustomFieldCommand(bugId, fieldName, fieldValue);

        var bug = Bug.Create(
            "Custom Field Bug",
            "Bug with custom fields",
            BugStatus.Open.Id,
            BugPriority.Medium.Id,
            BugSeverity.Medium.Name,
            projectId,
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        var project = Project.Create("Test Project", "Test Description").Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _projectRepository.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(project));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(fieldName, result.Value.Name);
        Assert.Equal(fieldValue, result.Value.Value);
        Assert.Equal("text", result.Value.FieldType);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAddingCustomField_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var projectId = new ProjectId(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var command = new AddCustomFieldCommand(bugId, "Browser Version", "Chrome 118");

        var bug = Bug.Create(
            "Browser Bug",
            "Browser specific issue",
            BugStatus.InProgress.Id,
            BugPriority.High.Id,
            BugSeverity.High.Name,
            projectId,
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        var project = Project.Create("Test Project", "Test Description").Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _projectRepository.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(project));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _bugRepository.Verify(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()), Times.Once);
        _projectRepository.Verify(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleCustomFieldsAdded_ShouldMaintainFieldList()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var projectId = new ProjectId(Guid.NewGuid());
        var userId = Guid.NewGuid();

        var bug = Bug.Create(
            "Multi Field Bug",
            "Bug with multiple custom fields",
            BugStatus.Open.Id,
            BugPriority.Low.Id,
            BugSeverity.Low.Name,
            projectId,
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        var project = Project.Create("Test Project", "Test Description").Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _projectRepository.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(project));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        var osField = new AddCustomFieldCommand(bugId, "Operating System", "Windows 11");
        var versionField = new AddCustomFieldCommand(bugId, "App Version", "2.1.0");

        // Act
        var osResult = await _sut.Handle(osField, CancellationToken.None);
        var versionResult = await _sut.Handle(versionField, CancellationToken.None);

        // Assert
        Assert.True(osResult.IsSuccess);
        Assert.True(versionResult.IsSuccess);
        Assert.Equal(2, bug.BugCustomFields.Count);
        // Note: We can't easily test the navigation properties in unit tests
        // so we'll just verify that the bug custom fields were added
    }

    [Fact]
    public async Task Handle_WhenCustomFieldAdded_ShouldSetCorrectProperties()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var projectId = new ProjectId(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var expectedName = "Test Field";
        var expectedValue = "Test Value";
        var command = new AddCustomFieldCommand(bugId, expectedName, expectedValue);

        var bug = Bug.Create(
            "Property Test Bug",
            "Testing custom field properties",
            BugStatus.Resolved.Id,
            BugPriority.Low.Id,
            BugSeverity.Low.Name,
            projectId,
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        var project = Project.Create("Test Project", "Test Description").Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _projectRepository.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(project));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedValue, result.Value.Value);
        Assert.Equal("text", result.Value.FieldType);
        Assert.Single(bug.BugCustomFields);

        var addedField = bug.BugCustomFields.First();
        Assert.Equal(expectedValue, addedField.Value);
        Assert.Equal(bug.Id, addedField.BugId);
    }
}
