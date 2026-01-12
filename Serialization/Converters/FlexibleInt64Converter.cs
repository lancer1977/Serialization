using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads a <see cref="long"/> from either a JSON number or a JSON string.
/// Accepts: 123, "123", " 123 ".
/// </summary>
public sealed class FlexibleInt64Converter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var n))
                return n;

            // Fall back: parse as double then cast (not ideal, but better than failing on "123.0")
            var d = reader.GetDouble();
            return checked((long)d);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                throw new JsonException("Cannot parse int64 from empty string.");

            if (long.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                return n;

            if (double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return checked((long)d);

            throw new JsonException($"Cannot parse int64 from '{s}'.");
        }

        throw new JsonException($"Expected number or string, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
