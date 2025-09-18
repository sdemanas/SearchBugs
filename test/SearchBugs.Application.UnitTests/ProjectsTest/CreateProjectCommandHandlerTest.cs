using Moq;
using SearchBugs.Application.Projects.CreateProject;
using SearchBugs.Domain;
using SearchBugs.Domain.Projects;

namespace SearchBugs.Application.UnitTests.ProjectsTest;

public class CreateProjectCommandHandlerTest
{
    private readonly Mock<IProjectRepository> _projectRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly CreateProjectCommandHandler _sut;

    public CreateProjectCommandHandlerTest()
    {
        _projectRepository = new();
        _unitOfWork = new();
        _sut = new CreateProjectCommandHandler(_projectRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldCreateProjectSuccessfully()
    {
        // Arrange
        var projectName = "Test Project";
        var description = "This is a test project";
        var command = new CreateProjectCommand(projectName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _projectRepository.Verify(x => x.Add(It.IsAny<Project>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCreatingProject_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var projectName = "Integration Project";
        var description = "Integration testing project";
        var command = new CreateProjectCommand(projectName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _projectRepository.Verify(x => x.Add(It.Is<Project>(p =>
            p.Name == projectName &&
            p.Description == description)), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProjectCreated_ShouldSetCorrectProperties()
    {
        // Arrange
        var expectedName = "Property Test Project";
        var expectedDescription = "Testing project properties";
        var command = new CreateProjectCommand(expectedName, expectedDescription);

        Project? capturedProject = null;
        _projectRepository.Setup(x => x.Add(It.IsAny<Project>()))
            .Callback<Project>(project => capturedProject = project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProject);
        Assert.Equal(expectedName, capturedProject.Name);
        Assert.Equal(expectedDescription, capturedProject.Description);
        Assert.True(capturedProject.Id.Value != Guid.Empty);
        Assert.True(capturedProject.CreatedOnUtc <= DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WhenMultipleProjectsCreated_ShouldCreateEachSuccessfully()
    {
        // Arrange
        var project1 = new CreateProjectCommand("Project 1", "First project");
        var project2 = new CreateProjectCommand("Project 2", "Second project");

        // Act
        var result1 = await _sut.Handle(project1, CancellationToken.None);
        var result2 = await _sut.Handle(project2, CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        _projectRepository.Verify(x => x.Add(It.IsAny<Project>()), Times.Exactly(2));
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenValidInput_ShouldNotThrowException()
    {
        // Arrange
        var command = new CreateProjectCommand("Exception Test", "Testing exceptions");

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _sut.Handle(command, CancellationToken.None));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_WhenProjectNameProvided_ShouldUseCorrectName()
    {
        // Arrange
        var specificName = "Very Specific Project Name";
        var command = new CreateProjectCommand(specificName, "Description");

        Project? capturedProject = null;
        _projectRepository.Setup(x => x.Add(It.IsAny<Project>()))
            .Callback<Project>(project => capturedProject = project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProject);
        Assert.Equal(specificName, capturedProject.Name);
    }

    [Fact]
    public async Task Handle_WhenProjectDescriptionProvided_ShouldUseCorrectDescription()
    {
        // Arrange
        var specificDescription = "This is a very detailed project description for testing purposes";
        var command = new CreateProjectCommand("Test Project", specificDescription);

        Project? capturedProject = null;
        _projectRepository.Setup(x => x.Add(It.IsAny<Project>()))
            .Callback<Project>(project => capturedProject = project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProject);
        Assert.Equal(specificDescription, capturedProject.Description);
    }

    [Fact]
    public async Task Handle_WhenProjectCreated_ShouldHaveValidId()
    {
        // Arrange
        var command = new CreateProjectCommand("ID Test", "Testing project ID generation");

        Project? capturedProject = null;
        _projectRepository.Setup(x => x.Add(It.IsAny<Project>()))
            .Callback<Project>(project => capturedProject = project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProject);
        Assert.True(capturedProject.Id.Value != Guid.Empty);
        Assert.True(capturedProject.CreatedOnUtc <= DateTime.UtcNow);
        Assert.True(capturedProject.CreatedOnUtc > DateTime.UtcNow.AddMinutes(-1));
    }
}
