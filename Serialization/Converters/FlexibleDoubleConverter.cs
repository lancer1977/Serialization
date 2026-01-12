using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads a <see cref="double"/> from either a JSON number or a JSON string.
/// Accepts: 1.23, "1.23", " 1.23 ".
/// </summary>
public sealed class FlexibleDoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                throw new JsonException("Cannot parse double from empty string.");

            if (double.TryParse(s.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var n))
                return n;

            throw new JsonException($"Cannot parse double from '{s}'.");
        }

        throw new JsonException($"Expected number or string, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
