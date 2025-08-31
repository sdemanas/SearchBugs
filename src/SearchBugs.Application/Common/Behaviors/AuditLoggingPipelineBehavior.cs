using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SearchBugs.Application.Common.Attributes;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain.AuditLogs;
using SearchBugs.Domain.Repositories;
using SearchBugs.Domain.Users;
using Shared.Messaging;
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

    // Maximum lengths for JSON data to prevent database issues
    private const int MaxRequestDataLength = 10000; // 10KB
    private const int MaxResponseDataLength = 50000; // 50KB

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
        // Skip logging for queries - only log commands/actions
        if (IsQuery(request))
        {
            return await next();
        }

        var stopwatch = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;
        var requestData = SerializeAndTruncate(request, MaxRequestDataLength);

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

            responseData = SerializeAndTruncate(response, MaxResponseDataLength);

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

    private static string SerializeAndTruncate<T>(T obj, int maxLength)
    {
        try
        {
            var sanitizedObj = RedactSensitiveProperties(obj);

            var json = JsonSerializer.Serialize(sanitizedObj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (json.Length <= maxLength)
            {
                return json;
            }

            // Truncate and add indicator
            var truncatedJson = json[..(maxLength - 50)]; // Leave space for truncation message
            return truncatedJson + "... [TRUNCATED - Original length: " + json.Length + " chars]";
        }
        catch (Exception)
        {
            return "[SERIALIZATION_ERROR]";
        }
    }

    /// <summary>
    /// Determines if the request is a query by checking if it implements IQuery interface
    /// </summary>
    private static bool IsQuery<T>(T request)
    {
        if (request == null) return false;

        var requestType = request.GetType();
        var interfaces = requestType.GetInterfaces();

        return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
    }

    /// <summary>
    /// Creates a copy of the object with sensitive properties redacted based on AuditIgnore attribute
    /// </summary>
    private static object RedactSensitiveProperties<T>(T obj)
    {
        if (obj == null) return obj!;

        var type = obj.GetType();

        // For primitive types and strings, return as-is
        if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) ||
            type == typeof(Guid) || type == typeof(decimal) || type.IsEnum)
        {
            return obj;
        }

        // For collections, process each item
        if (obj is System.Collections.IEnumerable enumerable && type != typeof(string))
        {
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(RedactSensitiveProperties(item));
            }
            return items;
        }

        // Create anonymous object with redacted properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var redactedProperties = new Dictionary<string, object?>();

        foreach (var property in properties)
        {
            try
            {
                var hasAuditIgnore = property.GetCustomAttribute<AuditIgnoreAttribute>() != null;
                var value = property.GetValue(obj);

                if (hasAuditIgnore)
                {
                    redactedProperties[property.Name] = "****";
                }
                else if (value != null)
                {
                    redactedProperties[property.Name] = RedactSensitiveProperties(value);
                }
                else
                {
                    redactedProperties[property.Name] = null;
                }
            }
            catch
            {
                // If we can't access the property, mark it as redacted
                redactedProperties[property.Name] = "****";
            }
        }

        return redactedProperties;
    }
}
