using Shared.Errors;

namespace SearchBugs.Application.Git;

internal static class GitValidationErrors
{
    internal static Error NameIsRequired => new("Git.NameIsRequired", "The git's name is required.");
    internal static Error DescriptionIsRequired => new("Git.DescriptionIsRequired", "The git's description is required.");
    internal static Error UrlIsRequired => new("Git.UrlIsRequired", "The git's url is required.");

    internal static Error GitRepoNotFound => new("Git.GitRepoNotFound", "The git repository was not found.");

    /// create error that can pass exception message
    /// 
    internal static Error SomeThingWentWrongWhenCreatingGitRepo(string message)
    {
        return new("Git.SomeThingWentWrongWhenCreatingGitRepo", $"Something went wrong when creating git repository: {message}");
    }

}
