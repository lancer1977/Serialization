using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads a <see cref="decimal"/> from either a JSON number or a JSON string.
/// Use decimal for money/precise values where floating point rounding is undesirable.
/// </summary>
public sealed class FlexibleDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // STJ has GetDecimal but it throws if the number was emitted as a double-like token.
            if (reader.TryGetDecimal(out var d))
                return d;

            // Fallback: parse from raw string representation.
            var raw = reader.GetDouble().ToString("R", CultureInfo.InvariantCulture);
            if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
                return dec;

            throw new JsonException("Cannot parse decimal from numeric token.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                throw new JsonException("Cannot parse decimal from empty string.");

            if (decimal.TryParse(s.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var dec))
                return dec;

            throw new JsonException($"Cannot parse decimal from '{s}'.");
        }

        throw new JsonException($"Expected number or string, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
