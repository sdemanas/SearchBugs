using System.Diagnostics;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SearchBugs.Domain.Git;
using SearchBugs.Infrastructure.Options;
namespace SearchBugs.Infrastructure.Services;

internal class GitHttpService : IGitHttpService
{
    private readonly GitOptions _gitOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private HttpContext _httpContext => _httpContextAccessor.HttpContext!;

    public GitHttpService(IOptions<GitOptions> gitOptions, IHttpContextAccessor httpContextAccessor)
    {
        _gitOptions = gitOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task DeleteRepository(string repositoryName, CancellationToken cancellationToken = default)
    {
        var gitPath = Path.Combine(_gitOptions.BasePath, repositoryName);
        if (Directory.Exists(gitPath))
        {
            Directory.Delete(gitPath, true);
        }
    }

    public async Task CreateRepository(string repositoryName, CancellationToken cancellationToken = default)
    {
        var gitPath = Path.Combine(_gitOptions.BasePath, repositoryName);
        if (!Directory.Exists(gitPath))
        {
            Directory.CreateDirectory(gitPath);
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init --bare",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = gitPath
            };
            process.Start();
            await process.WaitForExitAsync(cancellationToken);
        }
    }

    public async Task Handle(string repositoryName, string path, HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var gitPath = Path.Combine(_gitOptions.BasePath, $"{repositoryName}");
        ValidateRepository(gitPath);

        using var process = new Process();
        ConfigureProcess(repositoryName, process, gitPath, httpContext, path);

        process.Start();

        await HandleRequestPayload(httpContext, process, cancellationToken);
        await HandleResponse(httpContext, process, cancellationToken);
    }

    private void ValidateRepository(string gitPath)
    {
        if (!Directory.Exists(Path.Combine(gitPath, "objects")))
        {
            throw new DirectoryNotFoundException("Not a valid Git repository");
        }
    }

    private void ConfigureProcess(string repositoryName, Process process, string gitPath, HttpContext context, string path)
    {
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "http-backend",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = gitPath,
            Environment =
            {
                ["GIT_HTTP_EXPORT_ALL"] = "1",
                ["GIT_PROJECT_ROOT"] = _gitOptions.BasePath,
                ["PATH_INFO"] = $"/{repositoryName}.git/{path.TrimStart('/')}",
                ["REQUEST_METHOD"] = context.Request.Method,
                ["QUERY_STRING"] = context.Request.QueryString.Value?.TrimStart('?') ?? "",
                ["CONTENT_TYPE"] = context.Request.ContentType ?? GetDefaultContentType(path),
                ["CONTENT_LENGTH"] = GetContentLength(context.Request),
                ["REMOTE_ADDR"] = context.Connection.RemoteIpAddress?.ToString(),
                ["GIT_COMMITTER_NAME"] = context.User.Identity?.Name ?? "git-user"
            }
        };
    }

    private string GetDefaultContentType(string path)
    {
        return path.Contains("git-upload-pack") ?
            "application/x-git-upload-pack-request" :
            "application/x-git-receive-pack-request";
    }

    private string GetContentLength(HttpRequest request)
    {
        return request.ContentLength.HasValue ?
            request.ContentLength.Value.ToString() :
            "0";
    }

    private async Task HandleRequestPayload(HttpContext context, Process process, CancellationToken cancellationToken)
    {
        if (context.Request.ContentLength > 0)
        {
            await context.Request.Body.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
        }
        process.StandardInput.Close();
    }

    private async Task HandleResponse(HttpContext context, Process process, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = process.StartInfo.Environment["CONTENT_TYPE"];

        var outputPipe = PipeWriter.Create(context.Response.Body);
        var inputPipe = PipeReader.Create(process.StandardOutput.BaseStream);

        while (true)
        {
            var readResult = await inputPipe.ReadAsync(cancellationToken);

            foreach (var segment in readResult.Buffer)
            {
                await outputPipe.WriteAsync(segment, cancellationToken);
            }

            inputPipe.AdvanceTo(readResult.Buffer.End);

            if (readResult.IsCompleted)
            {
                break;
            }
        }

        await outputPipe.FlushAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Git error: {error}");
        }
    }
}



