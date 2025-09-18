using Microsoft.AspNetCore.Http;
using Shared.Messaging;

namespace SearchBugs.Application.Git.GitHttpServer;

public record GitHttpServerCommand(string Name,
    string Path,
    HttpContext HttpContext,
    CancellationToken CancellationToken = default) : ICommand;
