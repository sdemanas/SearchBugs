using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Authentications.StopImpersonate;

internal sealed class StopImpersonateCommandHandler : ICommandHandler<StopImpersonateCommand, StopImpersonateResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly ICurrentUserService _currentUserService;

    public StopImpersonateCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StopImpersonateResponse>> Handle(StopImpersonateCommand request, CancellationToken cancellationToken)
    {
        // Check if currently impersonating
        if (!_currentUserService.IsImpersonating)
        {
            return Result.Failure<StopImpersonateResponse>(new Error("NotImpersonating", "User is not currently impersonating anyone"));
        }

        // Get the original user (the actual logged-in user)
        var originalUserId = _currentUserService.ActualUserId;
        var originalUserResult = await _userRepository.GetByIdAsync(originalUserId, cancellationToken);

        if (originalUserResult.IsFailure || originalUserResult.Value is null)
        {
            return Result.Failure<StopImpersonateResponse>(UserErrors.NotFound(originalUserId));
        }

        // Generate a regular token (without impersonation claims)
        string token = _jwtProvider.GenerateJwtToken(originalUserResult.Value);

        return Result.Success(new StopImpersonateResponse(token));
    }
}
