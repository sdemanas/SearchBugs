using Moq;
using SearchBugs.Application.Authentications;
using SearchBugs.Application.Authentications.Register;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.AuthenicationsTest;

public class RegisterCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IPasswordHashingService> _hasher;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTest()
    {
        _userRepository = new();
        _hasher = new();
        _unitOfWork = new();
        _sut = new RegisterCommandHandler(_userRepository.Object, _hasher.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNotUnique_ShouldReturnFailure_WithDuplicateEmailError()
    {
        // Arrange
        var email = "email@email.com";
        var password = "password";

        var command = new RegisterCommand(email, password, "First", "Last");

        // IsEmailUniqueAsync returns success when email is NOT unique (email exists)
        _userRepository.Setup(x => x.IsEmailUniqueAsync(Email.Create(email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<User>(User.Create(Name.Create("First", "Last"), Email.Create(email), "hashedPassword").Value));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthValidationErrors.EmailAlreadyExists, result.Error);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_ShouldReturnSuccess()
    {
        // Arrange
        var email = "email@email.com";
        var password = "password";

        var command = new RegisterCommand(email, password, "First", "Last");

        // IsEmailUniqueAsync returns failure when email is unique (email doesn't exist)
        _userRepository.Setup(x => x.IsEmailUniqueAsync(Email.Create(email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(AuthValidationErrors.EmailAlreadyExists));

        _userRepository.Setup(x => x.GetRoleByIdAsync(Role.Guest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Role>(Role.Guest));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.GetRoleByIdAsync(Role.Guest.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
