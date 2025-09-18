using Moq;
using SearchBugs.Application.Users.DeleteUser;
using SearchBugs.Domain;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.UsersTest;

public class DeleteUserCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly DeleteUserCommandHandler _sut;

    public DeleteUserCommandHandlerTest()
    {
        _userRepository = new();
        _unitOfWork = new();
        _sut = new DeleteUserCommandHandler(_userRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(new NotFoundError("User.NotFound", "User not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.UserNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        var user = User.Create(
            Name.Create("John", "Doe"),
            Email.Create("john@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.Remove(user), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletingUser_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        var user = User.Create(
            Name.Create("Jane", "Smith"),
            Email.Create("jane@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.Remove(user), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidUserId_ShouldNotThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        var user = User.Create(
            Name.Create("Test", "User"),
            Email.Create("test@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _sut.Handle(command, CancellationToken.None));
        Assert.Null(exception);
    }
}
