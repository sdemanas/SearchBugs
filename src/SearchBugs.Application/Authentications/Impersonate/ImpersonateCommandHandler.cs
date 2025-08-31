using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.Users;
using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Authentications.Impersonate;

internal sealed class ImpersonateCommandHandler : ICommandHandler<ImpersonateCommand, ImpersonateResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly ICurrentUserService _currentUserService;

    public ImpersonateCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ImpersonateResponse>> Handle(ImpersonateCommand request, CancellationToken cancellationToken)
    {
        // Get the current user (the one trying to impersonate)
        var currentUserId = _currentUserService.ActualUserId;
        var currentUserResult = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);

        if (currentUserResult.IsFailure || currentUserResult.Value is null)
        {
            return Result.Failure<ImpersonateResponse>(UserErrors.NotFound(currentUserId));
        }

        // Get the user to impersonate
        var userToImpersonateResult = await _userRepository.GetByIdAsync(new UserId(request.UserIdToImpersonate), cancellationToken);

        if (userToImpersonateResult.IsFailure || userToImpersonateResult.Value is null)
        {
            return Result.Failure<ImpersonateResponse>(UserErrors.NotFound(new UserId(request.UserIdToImpersonate)));
        }

        // TODO: Add permission check here - only admins or users with specific permissions should be able to impersonate
        // if (!_currentUserService.HasPermission("CanImpersonate"))
        // {
        //     return Result.Failure<ImpersonateResponse>(UserErrors.InsufficientPermissions);
        // }

        // Generate impersonation token
        string token = _jwtProvider.GenerateImpersonationJwtToken(currentUserResult.Value, userToImpersonateResult.Value);

        return Result.Success(new ImpersonateResponse(token, userToImpersonateResult.Value.Email.Value));
    }
}
