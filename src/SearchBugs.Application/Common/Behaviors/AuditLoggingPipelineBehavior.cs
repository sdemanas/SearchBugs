using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.AuditLogs;
using SearchBugs.Domain.Repositories;
using SearchBugs.Domain.Users;
using Shared.Results;
using Shared.Time;

namespace SearchBugs.Application.Common.Behaviors;

public sealed class AuditLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingPipelineBehavior(
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;
        var requestData = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        string? responseData = null;
        bool isSuccess = false;
        string? errorMessage = null;

        try
        {
            var response = await next();

            isSuccess = response.IsSuccess;
            if (!response.IsSuccess)
            {
                errorMessage = response.Error?.Message;
            }

            responseData = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return response;
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessage = ex.Message;
            _logger.LogError(ex, "Unhandled exception occurred while processing request {RequestName}", requestName);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = GetIpAddress(httpContext);
            var userAgent = GetUserAgent(httpContext);

            UserId? userId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null;
            string? userName = _currentUserService.IsAuthenticated ? _currentUserService.Username : null;

            var auditLog = AuditLog.Create(
                requestName,
                requestData,
                responseData,
                isSuccess,
                errorMessage,
                stopwatch.Elapsed,
                userId,
                userName,
                ipAddress,
                userAgent,
                SystemTime.UtcNow);

            try
            {
                await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit log for request {RequestName}", requestName);
            }
        }
    }

    private static string GetIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null) return "Unknown";

        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        }

        return ipAddress ?? "Unknown";
    }

    private static string GetUserAgent(HttpContext? httpContext)
    {
        if (httpContext == null) return "Unknown";

        return httpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}
