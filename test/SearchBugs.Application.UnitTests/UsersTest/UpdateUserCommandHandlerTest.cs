using Moq;
using SearchBugs.Application.Users.UpdateUser;
using SearchBugs.Domain;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.UsersTest;

public class UpdateUserCommandHandlerTest
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTest()
    {
        _unitOfWork = new();
        _userRepository = new();
        _sut = new UpdateUserCommandHandler(_unitOfWork.Object, _userRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "John", "Doe");

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(new NotFoundError("User.NotFound", "User not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "John", "Doe");

        var user = User.Create(
            Name.Create("Original", "Name"),
            Email.Create("test@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("John", user.Name.FirstName);
        Assert.Equal("Doe", user.Name.LastName);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldCallRepositoryMethods()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "Jane", "Smith");

        var user = User.Create(
            Name.Create("Old", "Name"),
            Email.Create("jane@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
