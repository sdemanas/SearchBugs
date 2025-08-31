using FluentAssertions;
using Moq;
using SearchBugs.Application.Users.AssignRole;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.Users;

public class AssignRoleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AssignRoleCommandHandler _handler;

    public AssignRoleCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new AssignRoleCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _roleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = "Admin";
        var command = new AssignRoleCommand(userId, roleName);

        var user = CreateUserWithRole();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        var role = Role.Admin;
        _roleRepositoryMock
            .Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(role));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserRole.Conflict");
        result.Error.Message.Should().Be("User already has this role");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHaveRole_ShouldAssignRoleSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = "Developer";
        var command = new AssignRoleCommand(userId, roleName);

        var user = CreateUserWithRole(); // User has Admin role, not Developer
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        var role = Role.Developer;
        _roleRepositoryMock
            .Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(role));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Roles.Should().Contain(role);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = "Admin";
        var command = new AssignRoleCommand(userId, roleName);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(new Error("User.NotFound", "User not found")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = "NonExistentRole";
        var command = new AssignRoleCommand(userId, roleName);

        var user = CreateUserWithRole();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _roleRepositoryMock
            .Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Role>(new Error("Role.NotFound", "Role not found")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.NotFound");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static User CreateUserWithRole()
    {
        var name = Name.Create("John", "Doe");
        var email = Email.Create("john.doe@test.com");
        var user = User.Create(name, email, "password").Value;

        // Add Admin role to the user
        user.AddRole(Role.Admin);

        return user;
    }
}
