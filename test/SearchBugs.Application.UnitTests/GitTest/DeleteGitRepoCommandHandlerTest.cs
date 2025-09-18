using Moq;
using SearchBugs.Application.Git.DeleteGitRepo;
using SearchBugs.Domain;
using SearchBugs.Domain.Git;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Repositories;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.GitTest;

public class DeleteGitRepoCommandHandlerTest
{
    private readonly Mock<IGitRepository> _gitRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly DeleteGitRepoCommandHandler _sut;

    public DeleteGitRepoCommandHandlerTest()
    {
        _gitRepository = new();
        _unitOfWork = new();
        _sut = new DeleteGitRepoCommandHandler(_gitRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenRepositoryNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteGitRepoCommand("https://github.com/nonexistent/repo.git");

        _gitRepository.Setup(x => x.GetByUrlAsync(command.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Repository>(new Error("Git.NotFound", "Repository not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Git.GitRepoNotFound", result.Error.Code);
        _gitRepository.Verify(x => x.Remove(It.IsAny<Repository>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFound_ShouldDeleteSuccessfully()
    {
        // Arrange
        var url = "https://github.com/test/repo.git";
        var command = new DeleteGitRepoCommand(url);
        var repository = Repository.Create("test-repo", "Test repository", url, new ProjectId(Guid.NewGuid()));

        _gitRepository.Setup(x => x.GetByUrlAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(repository));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _gitRepository.Verify(x => x.Remove(repository), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCallingRemove_ShouldPassCorrectRepository()
    {
        // Arrange
        var url = "https://github.com/specific/repo.git";
        var command = new DeleteGitRepoCommand(url);
        var repository = Repository.Create("specific-repo", "Specific repository", url, new ProjectId(Guid.NewGuid()));

        _gitRepository.Setup(x => x.GetByUrlAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(repository));

        Repository? removedRepository = null;
        _gitRepository.Setup(x => x.Remove(It.IsAny<Repository>()))
            .Callback<Repository>(repo => removedRepository = repo)
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(removedRepository);
        Assert.Equal(repository.Name, removedRepository.Name);
        Assert.Equal(repository.Url, removedRepository.Url);
    }

    [Fact]
    public async Task Handle_WhenDeleteOperationExecuted_ShouldCallRepositoryGetByUrl()
    {
        // Arrange
        var specificUrl = "https://github.com/verify/get-by-url.git";
        var command = new DeleteGitRepoCommand(specificUrl);
        var repository = Repository.Create("verify-repo", "Verify repository", specificUrl, new ProjectId(Guid.NewGuid()));

        _gitRepository.Setup(x => x.GetByUrlAsync(specificUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(repository));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _gitRepository.Verify(x => x.GetByUrlAsync(
            It.Is<string>(url => url == specificUrl),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
