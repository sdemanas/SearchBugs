using MediatR;
using SearchBugs.Application.Users.Common;
using SearchBugs.Domain;
using SearchBugs.Domain.Roles;
using SearchBugs.Domain.Services;
using SearchBugs.Domain.Users;
using Shared.Results;

namespace SearchBugs.Application.Users.CreateUser;

internal sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashingService _passwordHashingService;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHashingService passwordHashingService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create name and email value objects
            var name = Name.Create(request.FirstName, request.LastName);
            var email = Email.Create(request.Email);

            // Check if user with email already exists
            var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (existingUser is not null)
            {
                return Result.Failure<CreateUserResponse>(UserValidationErrors.UserAlreadyExists);
            }

            // Hash password
            var hashedPassword = _passwordHashingService.HashPassword(request.Password);

            // Create user
            var userResult = User.Create(name, email, hashedPassword);
            if (userResult.IsFailure)
            {
                return Result.Failure<CreateUserResponse>(userResult.Error);
            }

            var user = userResult.Value;

            // Add roles if specified
            if (request.Roles is not null && request.Roles.Length > 0)
            {
                foreach (var roleName in request.Roles)
                {
                    var staticRole = Role.FromName(roleName);
                    if (staticRole is not null)
                    {
                        // Get the role from the database context to ensure it's tracked properly
                        var roleResult = await _userRepository.GetRoleByIdAsync(staticRole.Id, cancellationToken);
                        if (roleResult.IsSuccess)
                        {
                            user.AddRole(roleResult.Value);
                        }
                    }
                }
            }

            // Save user
            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateUserResponse(
                user.Id.Value.ToString(),
                user.Name.FirstName,
                user.Name.LastName,
                user.Email.Value,
                user.Roles.Select(r => new RoleDto(r.Id, r.Name)).ToArray(),
                user.CreatedOnUtc);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<CreateUserResponse>(new Shared.Errors.Error("User.ValidationError", ex.Message));
        }
    }
}
