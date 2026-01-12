using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Allows a property to accept either a single item <c>T</c> OR an array of <c>T</c>.
/// Produces a <see cref="List{T}"/>.
///
/// Example API shapes:
/// - "items": { ... }              (single)
/// - "items": [ { ... }, { ... } ] (array)
///
/// Usage:
/// <code>
/// public record Response(
///     [property: JsonConverter(typeof(SingleOrArrayConverter&lt;Thing&gt;))] List&lt;Thing&gt; Items
/// );
/// </code>
/// </summary>
public sealed class SingleOrArrayConverter<T> : JsonConverter<List<T>>
{
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
            return list ?? new List<T>();
        }

        // Not an array -> treat as single object/value
        var item = JsonSerializer.Deserialize<T>(ref reader, options);
        var result = new List<T>();
        if (item is not null)
            result.Add(item);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);
}
