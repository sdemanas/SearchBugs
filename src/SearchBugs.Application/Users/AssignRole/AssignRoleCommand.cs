using Shared.Messaging;

namespace SearchBugs.Application.Users.AssignRole;

public record AssignRoleCommand(Guid UserId, string Role) : ICommand;
