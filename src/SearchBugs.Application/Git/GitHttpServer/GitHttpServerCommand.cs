using Microsoft.AspNetCore.Http;
using Shared.Messaging;
using System.Net.Http;

namespace SearchBugs.Application.Git.GitHttpServer;

public record GitHttpServerCommand(string Name,
    string Path,
    HttpContext HttpContext,
    CancellationToken CancellationToken = default) : ICommand;
