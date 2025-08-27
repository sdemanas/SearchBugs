using Shared.Messaging;

namespace SearchBugs.Application.Git.CloneRepository;

public record CloneRepositoryCommand(string SourceUrl, string TargetPath) : ICommand;
