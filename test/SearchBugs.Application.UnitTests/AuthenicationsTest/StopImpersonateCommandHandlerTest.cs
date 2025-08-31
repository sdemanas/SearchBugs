using Moq;
using SearchBugs.Application.Authentications.StopImpersonate;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.AuthenicationsTest;

public class StopImpersonateCommandHandlerTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IJwtProvider> _jwtProvider;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly StopImpersonateCommandHandler _sut;

    public StopImpersonateCommandHandlerTest()
    {
        _userRepository = new();
        _jwtProvider = new();
        _currentUserService = new();
        _sut = new StopImpersonateCommandHandler(_userRepository.Object, _jwtProvider.Object, _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_WhenNotImpersonating_ShouldReturnFailure()
    {
        // Arrange
        var command = new StopImpersonateCommand();

        _currentUserService.Setup(x => x.IsImpersonating)
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NotImpersonating", result.Error.Code);
        Assert.Equal("User is not currently impersonating anyone", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WhenOriginalUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new StopImpersonateCommand();
        var originalUserId = new UserId(Guid.NewGuid());

        _currentUserService.Setup(x => x.IsImpersonating)
            .Returns(true);

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(originalUserId);

        _userRepository.Setup(x => x.GetByIdAsync(originalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<User>(UserErrors.NotFound(originalUserId)));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(originalUserId), result.Error);
    }

    [Fact]
    public async Task Handle_WhenOriginalUserFoundButNull_ShouldReturnFailure()
    {
        // Arrange
        var command = new StopImpersonateCommand();
        var originalUserId = new UserId(Guid.NewGuid());

        _currentUserService.Setup(x => x.IsImpersonating)
            .Returns(true);

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(originalUserId);

        _userRepository.Setup(x => x.GetByIdAsync(originalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<User>(null!));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(originalUserId), result.Error);
    }

    [Fact]
    public async Task Handle_WhenAllConditionsAreMet_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var command = new StopImpersonateCommand();
        var originalUserId = new UserId(Guid.NewGuid());
        var expectedToken = "regular-token";

        var originalUser = User.Create(Name.Create("Original", "User"), Email.Create("original@test.com"), "password").Value;

        _currentUserService.Setup(x => x.IsImpersonating)
            .Returns(true);

        _currentUserService.Setup(x => x.ActualUserId)
            .Returns(originalUserId);

        _userRepository.Setup(x => x.GetByIdAsync(originalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(originalUser));

        _jwtProvider.Setup(x => x.GenerateJwtToken(originalUser))
            .Returns(expectedToken);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value.Token);
    }
}
