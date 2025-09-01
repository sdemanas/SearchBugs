using Moq;
using SearchBugs.Application.Users;
using SearchBugs.Application.Users.CreateUser;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.UsersTest;

public class CreateUserCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPasswordHashingService> _passwordHashingService;
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTest()
    {
        _userRepository = new();
        _unitOfWork = new();
        _passwordHashingService = new();
        _sut = new CreateUserCommandHandler(_userRepository.Object, _unitOfWork.Object, _passwordHashingService.Object);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john.doe@example.com", "password123");
        var existingUser = User.Create(Name.Create("John", "Doe"), Email.Create("john.doe@example.com"), "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserValidationErrors.UserAlreadyExists, result.Error);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldHashPassword()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john.doe@example.com", "password123");
        var hashedPassword = "hashedPassword123";

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHashingService.Setup(x => x.HashPassword("password123"))
            .Returns(hashedPassword);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _passwordHashingService.Verify(x => x.HashPassword("password123"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExistAndNoRolesSpecified_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john.doe@example.com", "password123");
        var hashedPassword = "hashedPassword123";

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHashingService.Setup(x => x.HashPassword("password123"))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExistWithValidRoles_ShouldCreateUserWithSpecifiedRoles()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john.doe@example.com", "password123", new[] { "Admin", "Guest" });
        var hashedPassword = "hashedPassword123";

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHashingService.Setup(x => x.HashPassword("password123"))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Roles, r => r.Name == "Admin");
        Assert.Contains(result.Value.Roles, r => r.Name == "Guest");
    }

    [Fact]
    public async Task Handle_WhenUserCreatedSuccessfully_ShouldAddUserAndSaveChanges()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john.doe@example.com", "password123");
        var hashedPassword = "hashedPassword123";

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHashingService.Setup(x => x.HashPassword("password123"))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserCreatedSuccessfully_ShouldReturnUserInResponse()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john.doe@example.com", "password123");
        var hashedPassword = "hashedPassword123";

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHashingService.Setup(x => x.HashPassword("password123"))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value.FirstName);
        Assert.Equal("Doe", result.Value.LastName);
        Assert.Equal("john.doe@example.com", result.Value.Email);
    }
}
