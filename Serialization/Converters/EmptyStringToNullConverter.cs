using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Converts empty or whitespace-only JSON strings to <c>null</c> for <c>string?</c>.
///
/// Why this exists:
/// Some APIs use <c>""</c> to represent "missing" which is semantically closer to null.
/// This converter normalizes those responses so your domain model can treat "missing" as null.
/// </summary>
public sealed class EmptyStringToNullConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string or null, got {reader.TokenType}.");

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            return null;

        return s;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}
