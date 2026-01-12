using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads an <see cref="int"/> from either a JSON number or a JSON string.
/// Accepts: 123, "123", " 123 ".
/// </summary>
public sealed class FlexibleInt32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var n))
                return n;

            // Rare: number doesn't fit int32, or is encoded oddly.
            // Let STJ throw a clearer exception by re-reading as Int64 then casting safely.
            if (reader.TryGetInt64(out var i64))
                return checked((int)i64);

            throw new JsonException("Cannot parse int32 from numeric token.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                throw new JsonException("Cannot parse int32 from empty string.");

            if (int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                return n;

            // Some APIs send "123.0" as string; try double then cast if integral.
            if (double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return checked((int)d);

            throw new JsonException($"Cannot parse int32 from '{s}'.");
        }

        throw new JsonException($"Expected number or string, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
