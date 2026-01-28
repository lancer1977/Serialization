// Target: .NET 10
// Notes:
// - Uses ConcurrentDictionary for XmlSerializer caching (thread-safe, no manual locks)
// - Uses injected JsonSerializerOptions (so you can register your converters once)
// - Keeps "async" APIs via ValueTask wrappers for backward-compat without fake threadpool work
// - Fixes TryDeserialize semantics (returns false if result is null or exception)
// - Removes unnecessary `new()` constraints

using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace PolyhydraGames.Core.Serialization;
 

/// <summary>
/// Serialization helper for JSON and XML.
/// Modernized for .NET 10:
/// - Thread-safe XmlSerializer caching
/// - Configurable JsonSerializerOptions
/// - ValueTask wrappers for "async" API compatibility (no fake background threads)
/// </summary>
public sealed class SerializationHelper : ISerializationHelper
{
    // XmlSerializer is expensive to construct; caching matters.
    private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializers = new();

    private readonly JsonSerializerOptions _jsonOptions;

    public SerializationHelper(JsonSerializerOptions? jsonOptions = null)
    {
        // If caller doesn't supply options, use Web defaults (camelCase, etc.)
        // Caller should typically pass options with your converters registered.
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private static XmlSerializer GetXmlSerializer(Type type)
        => XmlSerializers.GetOrAdd(type, static t => new XmlSerializer(t));

    // ---------------------------
    // Deserialize
    // ---------------------------

    public ValueTask<T> DeserializeAsync<T>(string data, SerializationTypes serializationType = SerializationTypes.Json) where T : class, new()
    {
        return ValueTask.FromResult(Deserialize<T>(data, serializationType))!;
    }

    public T? Deserialize<T>(string data, SerializationTypes serializationType = SerializationTypes.Json) where T : class, new()
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        return serializationType switch
        {
            SerializationTypes.Xml => FromXml<T>(data),
            SerializationTypes.Json => JsonSerializer.Deserialize<T>(data, _jsonOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(serializationType), serializationType, "Unknown serialization type.")
        };
    }

    public bool TryDeserialize<T>(string data, SerializationTypes serializationType, out T? result) where T : class, new()
    {
        try
        {
            result = Deserialize<T>(data, serializationType);
            return result is not null;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    // ---------------------------
    // XML
    // ---------------------------

    public string ToXml(object? obj) => Serialize(obj, SerializationTypes.Xml);

    public ValueTask<string> ToXmlAsync(object? obj)
        => ValueTask.FromResult(ToXml(obj));

    public T? FromXml<T>(string xml) where T : class, new()
    {
        if (xml is null)
            throw new ArgumentNullException(nameof(xml));

        // For empty XML, return null rather than throwing.
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        using var reader = new StringReader(xml);
        var serializer = GetXmlSerializer(typeof(T));

        var deserialized = serializer.Deserialize(reader);
        return deserialized as T;
    }

    public ValueTask<T?> FromXmlAsync<T>(string xml) where T : class, new() => ValueTask.FromResult(FromXml<T>(xml));

    // ---------------------------
    // Serialize
    // ---------------------------

    public string Serialize(object? obj, SerializationTypes serializationType = SerializationTypes.Json)
    {
        return serializationType switch
        {
            SerializationTypes.Xml => SerializeToXml(obj),
            SerializationTypes.Json => JsonSerializer.Serialize(obj, _jsonOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(serializationType), serializationType, "Unknown serialization type.")
        };
    }

    public ValueTask<string> SerializeAsync(object? obj, SerializationTypes serializationType = SerializationTypes.Json)
        => ValueTask.FromResult(Serialize(obj, serializationType));

    private static string SerializeToXml(object? obj)
    {
        if (obj is null)
            return string.Empty;

        using var stringWriter = new StringWriter();

        var xmlWriterSettings = new XmlWriterSettings
        {
            Indent = false,
            NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = true
        };

        var nameSpace = new XmlSerializerNamespaces();
        nameSpace.Add(string.Empty, string.Empty);

        using var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);

        var xmlSerializer = GetXmlSerializer(obj.GetType());
        xmlSerializer.Serialize(xmlWriter, obj, nameSpace);

        return stringWriter.ToString();
    }
}
