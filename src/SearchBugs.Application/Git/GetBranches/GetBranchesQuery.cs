using Shared.Messaging;

namespace SearchBugs.Application.Git.GetBranches;

public record GetBranchesQuery(string RepoUrl) : IQuery<List<string>>;
