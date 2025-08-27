using MediatR;
using SearchBugs.Domain.Users;
using SearchBugs.Domain;
using Shared.Results;

namespace SearchBugs.Application.Users.DeleteUser;

internal sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure(UserValidationErrors.UserNotFound);
        }

        var user = userResult.Value;
        await _userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
