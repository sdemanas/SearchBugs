using Moq;
using SearchBugs.Application.Authentications.Impersonate;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.AuthenicationsTest;

public class ImpersonateCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IJwtProvider> _jwtProvider;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly ImpersonateCommandHandler _sut;

    public ImpersonateCommandHandlerTest()
    {
        _userRepository = new();
        _jwtProvider = new();
        _currentUserService = new();
        _sut = new ImpersonateCommandHandler(_userRepository.Object, _jwtProvider.Object, _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userIdToImpersonate = Guid.NewGuid();
        var currentUserId = new UserId(Guid.NewGuid());
        var command = new ImpersonateCommand(userIdToImpersonate);

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(currentUserId);

        _userRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(UserErrors.NotFound(currentUserId)));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(currentUserId), result.Error);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotAdmin_ShouldReturnInsufficientPermissions()
    {
        // Arrange
        var userIdToImpersonate = Guid.NewGuid();
        var currentUserId = new UserId(Guid.NewGuid());
        var command = new ImpersonateCommand(userIdToImpersonate);

        var currentUser = User.Create(Name.Create("Current", "User"), Email.Create("current@test.com"), "password").Value;
        // Add non-admin role
        currentUser.AddRole(Role.Guest);

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(currentUserId);

        _userRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(currentUser));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InsufficientPermissions, result.Error);
    }

    [Fact]
    public async Task Handle_WhenUserToImpersonateNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userIdToImpersonate = Guid.NewGuid();
        var targetUserId = new UserId(userIdToImpersonate);
        var currentUserId = new UserId(Guid.NewGuid());
        var command = new ImpersonateCommand(userIdToImpersonate);

        var currentUser = User.Create(Name.Create("Current", "User"), Email.Create("current@test.com"), "password").Value;
        // Add admin role
        currentUser.AddRole(Role.Admin);

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(currentUserId);

        _userRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(currentUser));

        _userRepository.Setup(x => x.GetByIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(UserErrors.NotFound(targetUserId)));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(targetUserId), result.Error);
    }

    [Fact]
    public async Task Handle_WhenAllConditionsAreMet_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var userIdToImpersonate = Guid.NewGuid();
        var targetUserId = new UserId(userIdToImpersonate);
        var currentUserId = new UserId(Guid.NewGuid());
        var command = new ImpersonateCommand(userIdToImpersonate);
        var expectedToken = "impersonate-token";

        var currentUser = User.Create(Name.Create("Current", "User"), Email.Create("current@test.com"), "password").Value;
        currentUser.AddRole(Role.Admin);

        var targetUser = User.Create(Name.Create("Target", "User"), Email.Create("target@test.com"), "password").Value;

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(currentUserId);

        _userRepository.Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(currentUser));

        _userRepository.Setup(x => x.GetByIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(targetUser));

        _jwtProvider.Setup(x => x.GenerateImpersonationJwtToken(currentUser, targetUser))
            .Returns(expectedToken);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value.Token);
    }
}
