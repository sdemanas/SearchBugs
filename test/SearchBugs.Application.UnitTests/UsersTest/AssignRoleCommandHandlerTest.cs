using Moq;
using SearchBugs.Application.Users.AssignRole;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.UsersTest;

public class AssignRoleCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly AssignRoleCommandHandler _sut;

    public AssignRoleCommandHandlerTest()
    {
        _userRepository = new();
        _roleRepository = new();
        _unitOfWork = new();
        _sut = new AssignRoleCommandHandler(_userRepository.Object, _unitOfWork.Object, _roleRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Admin.Name;
        var command = new AssignRoleCommand(userId, roleName);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(new NotFoundError("User.NotFound", "User not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = "NonExistentRole";
        var command = new AssignRoleCommand(userId, roleName);

        var user = User.Create(
            Name.Create("John", "Doe"),
            Email.Create("john@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _roleRepository.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Role>(new NotFoundError("Role.NotFound", "Role not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Role.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldAssignRoleSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Admin.Name;
        var command = new AssignRoleCommand(userId, roleName);

        var user = User.Create(
            Name.Create("John", "Doe"),
            Email.Create("john@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _roleRepository.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Role.Admin));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Admin.Name;
        var command = new AssignRoleCommand(userId, roleName);

        var user = User.Create(
            Name.Create("John", "Doe"),
            Email.Create("john@email.com"),
            "hashedPassword").Value;

        // Add the role to user first
        user.AddRole(Role.Admin);

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _roleRepository.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Role.Admin));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserRole.Conflict", result.Error.Code);
        Assert.Equal("User already has this role", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WhenAssigningRole_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleName = Role.Guest.Name;
        var command = new AssignRoleCommand(userId, roleName);

        var user = User.Create(
            Name.Create("Jane", "Smith"),
            Email.Create("jane@email.com"),
            "hashedPassword").Value;

        _userRepository.Setup(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _roleRepository.Setup(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Role.Guest));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepository.Verify(x => x.GetByIdAsync(new UserId(userId), It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetByNameAsync(roleName, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
