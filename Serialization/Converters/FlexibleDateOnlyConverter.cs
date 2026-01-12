using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads a <see cref="DateOnly"/> from common API formats:
/// - "yyyy-MM-dd"
/// - "MM/dd/yyyy"
/// - "yyyyMMdd"
///
/// If your API always uses ISO dates, prefer the default STJ behavior and skip this.
/// </summary>
public sealed class FlexibleDateOnlyConverter : JsonConverter<DateOnly>
{
    private static readonly string[] Formats =
    [
        "yyyy-MM-dd",
        "MM/dd/yyyy",
        "yyyyMMdd"
    ];

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string for DateOnly, got {reader.TokenType}.");

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            throw new JsonException("Cannot parse DateOnly from empty string.");

        if (DateOnly.TryParseExact(s.Trim(), Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;

        if (DateOnly.TryParse(s.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return d;

        throw new JsonException($"Cannot parse DateOnly from '{s}'.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
