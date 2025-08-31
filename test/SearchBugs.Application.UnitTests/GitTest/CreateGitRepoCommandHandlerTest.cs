using Moq;
using SearchBugs.Application.Git;
using SearchBugs.Application.Git.CreateGitRepo;
using SearchBugs.Domain;
using SearchBugs.Domain.Git;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Repositories;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.GitTest;

public class CreateGitRepoCommandHandlerTest
{
    private readonly Mock<IGitRepository> _gitRepository;
    private readonly Mock<IGitHttpService> _gitService;
    private readonly Mock<IProjectRepository> _projectRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly CreateGitRepoCommandHandler _sut;

    public CreateGitRepoCommandHandlerTest()
    {
        _gitRepository = new();
        _gitService = new();
        _projectRepository = new();
        _unitOfWork = new();
        _sut = new CreateGitRepoCommandHandler(_gitService.Object, _gitRepository.Object, _unitOfWork.Object, _projectRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldReturnFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new CreateGitRepoCommand("test-repo", "Description", "https://github.com/test/repo.git", projectId);

        _projectRepository.Setup(x => x.GetByIdAsync(new ProjectId(projectId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Project>(new Error("Project.NotFound", "Project not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Project.NotFound", result.Error.Code);
        _gitService.Verify(x => x.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _gitRepository.Verify(x => x.Add(It.IsAny<Repository>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldCreateRepositorySuccessfully()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectResult = Project.Create("Test Project", "Test Description");
        var command = new CreateGitRepoCommand("test-repository", "Test repository description", "https://github.com/user/test-repository.git", projectId);

        _projectRepository.Setup(x => x.GetByIdAsync(new ProjectId(projectId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _gitService.Verify(x => x.CreateRepository("test-repository", It.IsAny<CancellationToken>()), Times.Once);
        _gitRepository.Verify(x => x.Add(It.IsAny<Repository>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCreatingRepository_ShouldSetCorrectProperties()
    {
        // Arrange
        var expectedName = "specific-repo-name";
        var expectedDescription = "Specific repository description";
        var expectedUrl = "https://github.com/user/specific-repo-name.git";
        var expectedProjectId = Guid.NewGuid();
        var projectResult = Project.Create("Test Project", "Test Description");
        var command = new CreateGitRepoCommand(expectedName, expectedDescription, expectedUrl, expectedProjectId);

        _projectRepository.Setup(x => x.GetByIdAsync(new ProjectId(expectedProjectId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectResult);

        Repository? capturedRepository = null;
        _gitRepository.Setup(x => x.Add(It.IsAny<Repository>()))
            .Callback<Repository>(repo => capturedRepository = repo);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedRepository);
        Assert.Equal(expectedName, capturedRepository.Name);
        Assert.Equal(expectedDescription, capturedRepository.Description);
        Assert.Equal(expectedUrl, capturedRepository.Url);
        Assert.Equal(new ProjectId(expectedProjectId), capturedRepository.ProjectId);
    }

    [Fact]
    public async Task Handle_WhenDatabaseSaveFails_ShouldDeleteCreatedRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectResult = Project.Create("Test Project", "Test Description");
        var command = new CreateGitRepoCommand("failing-repo", "This will fail", "https://github.com/user/failing-repo.git", projectId);

        _projectRepository.Setup(x => x.GetByIdAsync(new ProjectId(projectId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectResult);

        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Git.SomeThingWentWrongWhenCreatingGitRepo", result.Error.Code);
        _gitService.Verify(x => x.CreateRepository("failing-repo", It.IsAny<CancellationToken>()), Times.Once);
        _gitService.Verify(x => x.DeleteRepository("failing-repo", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCallingGitService_ShouldPassCorrectRepositoryName()
    {
        // Arrange
        var repoName = "parameter-test-repo";
        var projectId = Guid.NewGuid();
        var projectResult = Project.Create("Test Project", "Test Description");
        var command = new CreateGitRepoCommand(repoName, "Parameter test description", "https://github.com/test/param-repo.git", projectId);

        _projectRepository.Setup(x => x.GetByIdAsync(new ProjectId(projectId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _gitService.Verify(x => x.CreateRepository(
            It.Is<string>(name => name == repoName),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
