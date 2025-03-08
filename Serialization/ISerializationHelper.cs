using System.Threading.Tasks;

namespace GCD.Core
{
    public interface ISerializationHelper
    {
        /// <summary>
        /// De-serializes a string into an instance of the object.
        /// </summary>
        /// <param name="data">The string containing the object data.</param>
        /// <param name="serializationType">The type of de-serialization to perform on the string.</param>
        /// <returns>The object created from the passed in string.</returns>
        T Deserialize<T>(string data, SerializationTypes serializationType = SerializationTypes.Json) where T : class, new();
        /// <summary>
        /// De-serializes a string into an instance of the object asynchronously.
        /// </summary>
        /// <param name="data">The string containing the object data.</param>
        /// <param name="serializationType">The type of de-serialization to perform on the string.</param>
        /// <returns>The object created from the passed in string.</returns>
        Task<T> DeserializeAsync<T>(string data, SerializationTypes serializationType = SerializationTypes.Json) where T : class, new();

        /// <summary>
        /// Trys to de-serializes a string into an instance of the object.
        /// </summary>
        /// <param name="data">The string containing the object data.</param>
        /// <param name="serializationType">The type of de-serialization to perform on the string.</param>
        /// <param name="result">The serialized object or null if the string can't be serialized.</param>
        /// <returns>True if the string can be serialized, otherwise false.</returns>
        bool TryDeserialize<T>(string data, SerializationTypes serializationType, out T result) where T : class, new();

        /// <summary>
        /// The most basic serialization of an object into an XML string.
        /// </summary>
        /// <returns>An XML string representing the object.</returns>
        string ToXml(object obj);
        /// <summary>
        /// The most basic serialization of an object into an XML string asynchronously.
        /// </summary>
        /// <returns>An XML string representing the object.</returns>

        Task<string> ToXmlAsync(object obj);
        /// <summary>
        /// The most basic de-serialization of an XML string into an object.
        /// </summary>
        /// <param name="xml">The XML string containing the object data.</param>
        /// <returns>The object created from the passed in XML string.</returns>
        T FromXml<T>(string xml) where T : class, new();
        /// <summary>
        /// The most basic de-serialization of an XML string into an object asynchronously.
        /// </summary>
        /// <param name="xml">The XML string containing the object data.</param>
        /// <returns>The object created from the passed in XML string.</returns>
        Task<T> FromXmlAsync<T>(string xml) where T : class, new();

        /// <summary>
        /// Serializes the object into a string.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="serializationType">The type of serialization to perform on the object.</param>
        /// <returns>A string representing the object.</returns>
        string Serialize(object obj, SerializationTypes serializationType);

        /// <summary>
        /// Serializes the object into a string.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="serializationType">The type of serialization to perform on the object asynchronously.</param>
        /// <returns>A string representing the object.</returns>
        Task<string> SerializeAsync(object obj, SerializationTypes serializationType);
    }
}