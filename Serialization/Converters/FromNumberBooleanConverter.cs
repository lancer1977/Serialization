using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyhydraGames.Core.Serialization.Converters
{
    public class FromNumberBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => reader.GetInt32() != 0,
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                _ => throw new JsonException()
            };
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}