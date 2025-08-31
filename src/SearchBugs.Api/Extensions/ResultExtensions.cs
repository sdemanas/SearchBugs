using Microsoft.AspNetCore.Mvc;

namespace SearchBugs.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this object result)
    {
        if (result == null)
            throw new InvalidOperationException("Result is null");

        var resultType = result.GetType();
        var isSuccessProperty = resultType.GetProperty("IsSuccess");
        var isSuccess = (bool)(isSuccessProperty?.GetValue(result) ?? false);

        if (!isSuccess)
        {
            // For failed results, return the result object directly with appropriate HTTP status
            var errorProperty = resultType.GetProperty("Error");
            var error = errorProperty?.GetValue(result);

            // You can customize the HTTP status code based on error type if needed
            return Results.BadRequest(result);
        }

        // For successful results, return the result object directly
        // This maintains the { isSuccess: true, value: T } structure
        return Results.Ok(result);
    }

    public static IResult ToCreatedResult(this object result, string location)
    {
        if (result == null)
            throw new InvalidOperationException("Result is null");

        var resultType = result.GetType();
        var isSuccessProperty = resultType.GetProperty("IsSuccess");
        var isSuccess = (bool)(isSuccessProperty?.GetValue(result) ?? false);

        if (!isSuccess)
        {
            // For failed results, return the result object directly with appropriate HTTP status
            return Results.BadRequest(result);
        }

        // For successful results, return the result object directly with Created status
        return Results.Created(location, result);
    }
}
