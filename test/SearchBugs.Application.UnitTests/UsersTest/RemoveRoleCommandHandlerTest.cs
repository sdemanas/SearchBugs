using Moq;
using SearchBugs.Application.Users;
using SearchBugs.Application.Users.RemoveRole;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.UsersTest;

public class RemoveRoleCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly RemoveRoleCommandHandler _sut;

    public RemoveRoleCommandHandlerTest()
    {
        _userRepository = new();
        _unitOfWork = new();
        _sut = new RemoveRoleCommandHandler(_userRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Admin.Name;
        var command = new RemoveRoleCommand(userId, roleName);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(UserValidationErrors.UserNotFound));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserValidationErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenRoleIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invalidRoleName = "InvalidRole";
        var command = new RemoveRoleCommand(userId, invalidRoleName);

        var user = User.Create(
            Name.Create("John", "Doe"),
            Email.Create("john@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserValidationErrors.InvalidRole, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldRemoveRoleSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Admin.Name;
        var command = new RemoveRoleCommand(userId, roleName);

        var user = User.Create(
            Name.Create("John", "Doe"),
            Email.Create("john@email.com"),
            "hashedPassword").Value;

        // Add the role first so we can remove it
        user.AddRole(Role.Admin);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _userRepository.Setup(x => x.GetRoleByIdAsync(Role.Admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Role.Admin));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRemovingRole_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Developer.Name;
        var command = new RemoveRoleCommand(userId, roleName);

        var user = User.Create(
            Name.Create("Jane", "Smith"),
            Email.Create("jane@email.com"),
            "hashedPassword").Value;

        // Add the role first
        user.AddRole(Role.Developer);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _userRepository.Setup(x => x.GetRoleByIdAsync(Role.Developer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Role.Developer));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasMultipleRoles_ShouldRemoveOnlySpecifiedRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleToRemove = Role.Developer.Name;
        var command = new RemoveRoleCommand(userId, roleToRemove);

        var user = User.Create(
            Name.Create("Multi", "Role"),
            Email.Create("multi@email.com"),
            "hashedPassword").Value;

        // Add multiple roles
        user.AddRole(Role.Admin);
        user.AddRole(Role.Developer);
        user.AddRole(Role.Reporter);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _userRepository.Setup(x => x.GetRoleByIdAsync(Role.Developer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Role.Developer));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Role.Developer, user.Roles);
        Assert.Contains(Role.Admin, user.Roles);
        Assert.Contains(Role.Reporter, user.Roles);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
