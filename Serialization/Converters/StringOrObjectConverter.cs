using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Handles union JSON shapes where an API returns either:
/// - a string: "something"
/// - an object: {{ ... }}
///
/// This converter maps to a wrapper type so your domain model stays explicit.
///
/// Usage:
/// <code>
/// public record Response(
///   [property: JsonConverter(typeof(StringOrObjectConverter&lt;SomeDto&gt;))]
///   StringOrObject&lt;SomeDto&gt; Value
/// );
/// </code>
/// </summary>
public sealed class StringOrObjectConverter<T> : JsonConverter<StringOrObject<T>>
{
    public override StringOrObject<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new StringOrObject<T>(reader.GetString()),
            JsonTokenType.StartObject => new StringOrObject<T>(JsonSerializer.Deserialize<T>(ref reader, options)),
            JsonTokenType.Null => new StringOrObject<T>((string?)null),
            _ => throw new JsonException($"Expected string or object, got {reader.TokenType}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, StringOrObject<T> value, JsonSerializerOptions options)
    {
        if (value.StringValue is not null)
        {
            writer.WriteStringValue(value.StringValue);
            return;
        }

        if (value.ObjectValue is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.ObjectValue, options);
    }
}