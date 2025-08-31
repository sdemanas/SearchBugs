using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Errors;
using Shared.Results;
using Shared.Exceptions;

namespace SearchBugs.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var exceptionDetails = GetExceptionDetails(exception);

            // Create a Result failure response based on exception type
            Result errorResult = exception switch
            {
                FluentValidation.ValidationException fluentValidationException => CreateFluentValidationResult(fluentValidationException),
                Shared.Exceptions.ValidationException customValidationException => CreateCustomValidationResult(customValidationException),
                UnauthorizedAccessException unauthorizedException => Result.Failure(new Error("Auth.Unauthorized", unauthorizedException.Message)),
                ArgumentException argumentException => Result.Failure(new Error("Argument.Invalid", argumentException.Message)),
                InvalidOperationException invalidOperationException => Result.Failure(new Error("Operation.Invalid", invalidOperationException.Message)),
                _ => Result.Failure(new Error(exceptionDetails.Type, exceptionDetails.Detail))
            };

            context.Response.StatusCode = exceptionDetails.Status;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(errorResult);
        }
    }

    private static Result CreateFluentValidationResult(FluentValidation.ValidationException validationException)
    {
        // For multiple validation errors, we'll return the first error in the Result format
        // and include all errors in a custom error message
        var errors = validationException.Errors.ToArray();
        if (errors.Length == 1)
        {
            var error = errors[0];
            return Result.Failure(new Error(error.ErrorCode ?? "Validation.Error", error.ErrorMessage));
        }

        // For multiple errors, create a comprehensive error message
        var errorMessages = string.Join("; ", errors.Select(e => e.ErrorMessage));
        return Result.Failure(new Error("Validation.MultipleErrors", errorMessages));
    }

    private static Result CreateCustomValidationResult(Shared.Exceptions.ValidationException validationException)
    {
        // For custom validation errors
        var errors = validationException.Errors.ToArray();
        if (errors.Length == 1)
        {
            var error = errors[0];
            return Result.Failure(new Error("Validation.Error", error.ErrorMessage));
        }

        // For multiple errors, create a comprehensive error message
        var errorMessages = string.Join("; ", errors.Select(e => e.ErrorMessage));
        return Result.Failure(new Error("Validation.MultipleErrors", errorMessages));
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException validationException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "Validation.Error",
                "Validation error",
                "One or more validation errors has occurred",
                validationException.Errors.Select(e => new { Code = e.ErrorCode, Message = e.ErrorMessage })),
            Shared.Exceptions.ValidationException customValidationException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "Validation.Error",
                "Validation error",
                "One or more validation errors has occurred",
                customValidationException.Errors.Select(e => new { Property = e.PropertyName, Message = e.ErrorMessage })),
            UnauthorizedAccessException => new ExceptionDetails(
                StatusCodes.Status401Unauthorized,
                "Auth.Unauthorized",
                "Unauthorized",
                "Access is denied due to invalid credentials",
                null),
            ArgumentException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "Argument.Invalid",
                "Invalid argument",
                "One or more arguments are invalid",
                null),
            InvalidOperationException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "Operation.Invalid",
                "Invalid operation",
                "The requested operation is not valid in the current state",
                null),
            _ => new ExceptionDetails(
                StatusCodes.Status500InternalServerError,
                "Server.Error",
                "Server error",
                "An unexpected error has occurred",
                null)
        };
    }

    internal record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors);
}