using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters;

/// <summary>
/// Reads an enum from either a string or number token.
///
/// Why this exists:
/// - Some APIs send enums as strings ("Active")
/// - Others send ints (1)
/// - Some evolve and introduce new values that your client doesn't know yet.
///
/// With <paramref name="unknownMapsToDefault"/>, unknown values won't crash your client; they'll map to default(TEnum).
///
/// Usage (global or per-property):
/// <code>
/// options.Converters.Add(new SafeEnumConverter&lt;MyEnum&gt;(unknownMapsToDefault: true));
/// </code>
/// </summary>
public sealed class SafeEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly bool _unknownMapsToDefault;
    private readonly bool _ignoreCase;

    public SafeEnumConverter(bool unknownMapsToDefault = false, bool ignoreCase = true)
    {
        _unknownMapsToDefault = unknownMapsToDefault;
        _ignoreCase = ignoreCase;
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return default;

                if (Enum.TryParse<TEnum>(s.Trim(), _ignoreCase, out var e))
                    return e;

                if (_unknownMapsToDefault)
                    return default;

                throw new JsonException($"Unknown enum value '{s}' for {typeof(TEnum).Name}.");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out var i))
                {
                    var e = (TEnum)Enum.ToObject(typeof(TEnum), i);
                    if (Enum.IsDefined(typeof(TEnum), e) || _unknownMapsToDefault)
                        return e;
                }

                if (_unknownMapsToDefault)
                    return default;

                throw new JsonException($"Unknown numeric enum value for {typeof(TEnum).Name}.");
            }

            if (reader.TokenType == JsonTokenType.Null)
                return default;

            throw new JsonException($"Expected string or number for enum {typeof(TEnum).Name}, got {reader.TokenType}.");
        }
        catch (Exception) when (_unknownMapsToDefault)
        {
            return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
