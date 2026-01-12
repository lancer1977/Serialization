using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads boolean values that may be encoded as:
/// - JSON booleans: <c>true</c> / <c>false</c>
/// - Numbers: <c>1</c> / <c>0</c>
/// - Strings: "true"/"false", "1"/"0", "yes"/"no", "y"/"n" (case-insensitive, trims whitespace)
///
/// Useful when an API is inconsistent across endpoints or versions.
/// </summary>
public sealed class FlexibleBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.String => ReadString(reader.GetString()),
            _ => throw new JsonException($"Expected boolean/number/string, got {reader.TokenType}.")
        };
    }

    private static bool ReadNumber(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt64(out var i))
            return i != 0;

        // Fall back: treat any non-zero as true
        var d = reader.GetDouble();
        return Math.Abs(d) > double.Epsilon;
    }

    private static bool ReadString(string? raw)
    {
        if (raw is null) return false;

        var s = raw.Trim();

        if (bool.TryParse(s, out var b))
            return b;

        // Common variations
        if (string.Equals(s, "1", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(s, "0", StringComparison.OrdinalIgnoreCase)) return false;

        if (string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(s, "no", StringComparison.OrdinalIgnoreCase)) return false;

        if (string.Equals(s, "y", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(s, "n", StringComparison.OrdinalIgnoreCase)) return false;

        throw new JsonException($"Cannot parse boolean from '{raw}'.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);
}