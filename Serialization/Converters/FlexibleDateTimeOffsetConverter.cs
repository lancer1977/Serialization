using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads a <see cref="DateTimeOffset"/> from common REST API formats:
/// - ISO-8601 strings: "2026-01-12T05:30:00Z"
/// - RFC1123 strings: "Mon, 12 Jan 2026 05:30:00 GMT"
/// - Unix epoch seconds or milliseconds:
///   - number: 1700000000 or 1700000000000
///   - string: "1700000000"
///
/// Many APIs evolve over time and change timestamp formats; this keeps your client resilient.
/// </summary>
public sealed class FlexibleDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private static readonly string[] KnownFormats =
    [
        "O",                          // Round-trip ISO 8601
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
        "r",                          // RFC1123
        "yyyy-MM-dd HH:mm:ssK",
        "yyyy-MM-dd"
    ];

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => ReadFromString(reader.GetString()),
            JsonTokenType.Number => ReadFromNumber(ref reader),
            _ => throw new JsonException($"Expected string or number for timestamp, got {reader.TokenType}.")
        };
    }

    private static DateTimeOffset ReadFromNumber(ref Utf8JsonReader reader)
    {
        // Assume epoch seconds or milliseconds depending on magnitude.
        if (!reader.TryGetInt64(out var n))
        {
            // If it is a floating numeric, read as double and cast.
            n = checked((long)reader.GetDouble());
        }

        return FromEpochGuessingSecondsOrMilliseconds(n);
    }

    private static DateTimeOffset ReadFromString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new JsonException("Cannot parse timestamp from empty string.");

        var s = raw.Trim();

        // If the string is purely digits, treat as epoch.
        if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var epoch))
            return FromEpochGuessingSecondsOrMilliseconds(epoch);

        // Try known formats first (fast path)
        if (DateTimeOffset.TryParseExact(s, KnownFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
            return dto;

        // Final fallback: let the runtime parse (may accept more variants)
        if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dto))
            return dto;

        throw new JsonException($"Cannot parse DateTimeOffset from '{raw}'.");
    }

    private static DateTimeOffset FromEpochGuessingSecondsOrMilliseconds(long epoch)
    {
        // Rough heuristic: milliseconds will be much larger.
        // 10^12 ms ~ 2001-09-09; 10^10 s ~ 2286-11-20
        if (epoch >= 1_000_000_000_000L)
            return DateTimeOffset.FromUnixTimeMilliseconds(epoch);

        return DateTimeOffset.FromUnixTimeSeconds(epoch);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
}
