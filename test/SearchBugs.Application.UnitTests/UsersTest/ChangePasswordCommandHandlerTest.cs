using Moq;
using SearchBugs.Application.Users;
using SearchBugs.Application.Users.ChangePassword;
using SearchBugs.Domain;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.UsersTest;

public class ChangePasswordCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPasswordHashingService> _passwordHashingService;
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTest()
    {
        _userRepository = new();
        _unitOfWork = new();
        _passwordHashingService = new();
        _sut = new ChangePasswordCommandHandler(_userRepository.Object, _unitOfWork.Object, _passwordHashingService.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "currentPassword", "newPassword");

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(UserValidationErrors.UserNotFound));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserValidationErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "wrongCurrentPassword", "newPassword");
        var user = User.Create(Name.Create("Test", "User"), Email.Create("test@test.com"), "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _passwordHashingService.Setup(x => x.VerifyPassword("wrongCurrentPassword", "hashedPassword"))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserValidationErrors.InvalidCurrentPassword, result.Error);
    }

    [Fact]
    public async Task Handle_WhenAllConditionsAreMet_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "currentPassword", "newPassword");
        var user = User.Create(Name.Create("Test", "User"), Email.Create("test@test.com"), "hashedPassword").Value;
        var newHashedPassword = "newHashedPassword";

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _passwordHashingService.Setup(x => x.VerifyPassword("currentPassword", "hashedPassword"))
            .Returns(true);

        _passwordHashingService.Setup(x => x.HashPassword("newPassword"))
            .Returns(newHashedPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAllConditionsAreMet_ShouldHashNewPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "currentPassword", "newPassword");
        var user = User.Create(Name.Create("Test", "User"), Email.Create("test@test.com"), "hashedPassword").Value;
        var newHashedPassword = "newHashedPassword";

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _passwordHashingService.Setup(x => x.VerifyPassword("currentPassword", "hashedPassword"))
            .Returns(true);

        _passwordHashingService.Setup(x => x.HashPassword("newPassword"))
            .Returns(newHashedPassword);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _passwordHashingService.Verify(x => x.HashPassword("newPassword"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAllConditionsAreMet_ShouldVerifyCurrentPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "currentPassword", "newPassword");
        var user = User.Create(Name.Create("Test", "User"), Email.Create("test@test.com"), "hashedPassword").Value;
        var newHashedPassword = "newHashedPassword";

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _passwordHashingService.Setup(x => x.VerifyPassword("currentPassword", "hashedPassword"))
            .Returns(true);

        _passwordHashingService.Setup(x => x.HashPassword("newPassword"))
            .Returns(newHashedPassword);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _passwordHashingService.Verify(x => x.VerifyPassword("currentPassword", "hashedPassword"), Times.Once);
    }
}
