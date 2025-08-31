using Microsoft.AspNetCore.Mvc;

namespace SearchBugs.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this object result)
    {
        if (result == null)
            throw new InvalidOperationException("Result is null");

        var resultType = result.GetType();

        // Debug logging
        Console.WriteLine($"Result type: {resultType.Name}");
        Console.WriteLine($"Is generic: {resultType.IsGenericType}");
        Console.WriteLine($"Properties: {string.Join(", ", resultType.GetProperties().Select(p => p.Name))}");

        var isSuccessProperty = resultType.GetProperty("IsSuccess");
        var isSuccess = (bool)(isSuccessProperty?.GetValue(result) ?? false);

        Console.WriteLine($"IsSuccess: {isSuccess}");

        if (!isSuccess)
        {
            var errorProperty = resultType.GetProperty("Error");
            var error = errorProperty?.GetValue(result);
            throw new InvalidOperationException($"Operation failed: {error}");
        }

        // Check if this result has a Value property (Result<T>)
        var valueProperty = resultType.GetProperty("Value");
        Console.WriteLine($"Has Value property: {valueProperty != null}");

        if (valueProperty != null)
        {
            var value = valueProperty.GetValue(result);
            Console.WriteLine($"Value: {value}");
            return Results.Ok(value);
        }

        // This is a Result (not Result<T>), so just return Ok with no content
        return Results.Ok();
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
            var errorProperty = resultType.GetProperty("Error");
            var error = errorProperty?.GetValue(result);
            throw new InvalidOperationException($"Operation failed: {error}");
        }

        // Check if this result has a Value property (Result<T>)
        var valueProperty = resultType.GetProperty("Value");
        if (valueProperty != null)
        {
            var value = valueProperty.GetValue(result);
            return Results.Created(location, value);
        }

        // This is a Result (not Result<T>), so just return Created with no content
        return Results.Created(location, null);
    }
}
