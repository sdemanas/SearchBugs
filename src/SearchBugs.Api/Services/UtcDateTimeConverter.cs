using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SearchBugs.Api.Services;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTimeString = reader.GetString();
        if (string.IsNullOrEmpty(dateTimeString))
            return default;

        if (DateTime.TryParse(dateTimeString, null, DateTimeStyles.RoundtripKind, out var dateTime))
        {
            // If the DateTime doesn't have Kind specified, assume it's UTC
            return dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }

        throw new JsonException($"Unable to parse DateTime: {dateTimeString}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always serialize as UTC ISO string
        var utcDateTime = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        writer.WriteStringValue(utcDateTime.ToString("O")); // ISO 8601 format
    }
}

public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var dateTimeString = reader.GetString();
        if (string.IsNullOrEmpty(dateTimeString))
            return null;

        if (DateTime.TryParse(dateTimeString, null, DateTimeStyles.RoundtripKind, out var dateTime))
        {
            // If the DateTime doesn't have Kind specified, assume it's UTC
            return dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }

        throw new JsonException($"Unable to parse DateTime: {dateTimeString}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Always serialize as UTC ISO string
        var utcDateTime = value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime();
        writer.WriteStringValue(utcDateTime.ToString("O")); // ISO 8601 format
    }
}
