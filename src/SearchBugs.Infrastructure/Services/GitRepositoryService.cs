﻿using LibGit2Sharp;
using Microsoft.Extensions.Options;
using SearchBugs.Domain.Git;
using SearchBugs.Infrastructure.Options;
using Shared.Results;

namespace SearchBugs.Infrastructure.Services;

internal sealed partial class GitRepositoryService : IGitRepositoryService
{
    private readonly GitOptions _gitOptions;
    private readonly string _basePath;

    public GitRepositoryService(IOptions<GitOptions> gitOptions)
    {
        _gitOptions = gitOptions.Value;
        _basePath = _gitOptions.BasePath;
    }

    public Result<IEnumerable<GitTreeItem>> ListTree(string commitSha, string repoPath)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        if (!Directory.Exists(_repoPath)) return Result.Failure<IEnumerable<GitTreeItem>>(GitErrors.RepositoryNotFound);
        using (var repo = new Repository(_repoPath))
        {
            var commit = repo.Lookup<Commit>(commitSha) ?? repo.Head.Tip;
            if (commit == null) return Result.Failure<IEnumerable<GitTreeItem>>(GitErrors.InvalidCommitPath);
            var tree = commit.Tree;

            return tree.Select(entry => new GitTreeItem
            {
                Path = entry.Path,
                Name = entry.Name,
                Type = entry.TargetType.ToString()
            }).ToList();
        }
    }

    public Result<string> GetFileContent(string repoPath, string commitSha, string filePath)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var commit = repo.Lookup<Commit>(commitSha);
            if (commit == null) return Result.Failure<string>(GitErrors.InvalidCommitPath);

            var blob = commit[filePath]?.Target as Blob;
            if (blob == null) return Result.Failure<string>(GitErrors.FileNotFound);

            return blob.GetContentText();
        }
    }

    public Result CommitChanges(string repoPath, string authorName, string authorEmail, string commitMessage)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);

        using (var repo = new Repository(_repoPath))
        {
            Commands.Stage(repo, "*");

            var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
            var committer = author;

            // Commit to the repository
            repo.Commit(commitMessage, author, committer);
        }
        return Result.Success();
    }

    public Result<IEnumerable<FileDiff>> GetCommitDiff(string repoPath, string commitSha)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var commit = repo.Lookup<Commit>(commitSha);
            if (commit == null) return Result.Failure<IEnumerable<FileDiff>>(GitErrors.InvalidCommitPath);

            var diffs = new List<FileDiff>();

            if (commit.Parents.Any())
            {
                var parentCommit = commit.Parents.First();
                var patch = repo.Diff.Compare<Patch>(parentCommit.Tree, commit.Tree);

                diffs.AddRange(patch.Select(p => new FileDiff
                {
                    FilePath = p.Path,
                    OldPath = p.OldPath,
                    Status = p.Status.ToString(),
                    Patch = p.Patch
                }));
            }

            return diffs;
        }
    }

    public Result<IEnumerable<FileBlame>> GetFileBlame(string repoPath, string filePath)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var blame = repo.Blame(filePath);

            return blame.Select(h => new FileBlame
            {
                LineNumber = h.FinalStartLineNumber,
                CommitSha = h.FinalCommit.Sha,
                Author = h.FinalSignature.Name,
                Email = h.FinalSignature.Email,
                Date = h.FinalSignature.When.DateTime,
                LineContent = h.LineCount,
            }).ToList();
        }
    }

    public Result<MergeResult> MergeBranches(string repoPath, string sourceBranchName, string targetBranchName, string mergerName, string mergerEmail)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var sourceBranch = repo.Branches[sourceBranchName];
            var targetBranch = repo.Branches[targetBranchName];

            if (sourceBranch == null || targetBranch == null) return Result.Failure<MergeResult>(GitErrors.BranchNotFound);

            // Merge
            var merger = new Signature(mergerName, mergerEmail, DateTimeOffset.Now);
            var result = repo.Merge(sourceBranch, merger);

            return new MergeResult
            {
                Status = result.Status.ToString(),
                CommitSha = result.Commit?.Sha ?? string.Empty
            };
        }
    }

    public Result<IEnumerable<FileDiff>> CompareCommits(string repoPath, string baseCommitSha, string compareCommitSha)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var baseCommit = repo.Lookup<Commit>(baseCommitSha);
            var compareCommit = repo.Lookup<Commit>(compareCommitSha);

            if (baseCommit == null || compareCommit == null) return Result.Failure<IEnumerable<FileDiff>>(GitErrors.CommitNotFound);

            var patch = repo.Diff.Compare<Patch>(baseCommit.Tree, compareCommit.Tree);

            return patch.Select(p => new FileDiff
            {
                FilePath = p.Path,
                OldPath = p.OldPath,
                Status = p.Status.ToString(),
                Patch = p.Patch
            }).ToList();
        }
    }

    public Result CheckoutBranch(string repoPath, string branchName)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var branch = repo.Branches[branchName];
            if (branch == null) return Result.Failure(GitErrors.BranchNotFound);

            Commands.Checkout(repo, branch);
        }
        return Result.Success();
    }

    public Result<IEnumerable<Contributor>> GetContributors(string repoPath)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        using (var repo = new Repository(_repoPath))
        {
            var contributors = repo.Commits
                .Select(c => c.Author)
                .GroupBy(a => a.Email)
                .Select(g => new Contributor
                {
                    Name = g.First().Name,
                    Email = g.Key,
                    CommitCount = g.Count()
                })
                .ToList();

            return contributors;
        }
    }

    public Result<IEnumerable<string>> GetBranches(string repoPath)
    {
        var _repoPath = Path.Combine(_basePath, repoPath);
        if (!Directory.Exists(_repoPath)) return Result.Failure<IEnumerable<string>>(GitErrors.RepositoryNotFound);

        using (var repo = new Repository(_repoPath))
        {
            var branches = repo.Branches
                .Where(b => !b.IsRemote)
                .Select(b => b.FriendlyName)
                .ToList();

            return Result.Success(branches.AsEnumerable());
        }
    }

    public Result CloneRepository(string sourceUrl, string targetPath)
    {
        try
        {
            var _targetPath = Path.Combine(_basePath, targetPath);
            if (Directory.Exists(_targetPath))
            {
                return Result.Failure(GitErrors.RepositoryAlreadyExists);
            }

            Repository.Clone(sourceUrl, _targetPath);
            return Result.Success();
        }
        catch (LibGit2SharpException ex)
        {
            return Result.Failure(GitErrors.CloneFailure(ex.Message));
        }
    }
}

