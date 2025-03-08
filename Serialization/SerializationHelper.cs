using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace GCD.Core.Serialization
{
    [DebuggerStepThrough]
    public abstract class InterfacedSingletonBase<T, TI> where T : TI, new() where TI : class
    {
        /// <summary>
        /// Concurrency Locking Object.
        /// </summary>
        protected static readonly object _ConcurrencyLock = new object();

        /// <summary>
        /// Holds the current active singleton object.
        /// </summary>
        private static volatile TI _Current;

        /// <summary>
        /// Gets or sets the Singleton Current Static Property.
        /// </summary>
        public static TI Current
        {
            get
            {
                if (_Current == null)
                {
                    lock (_ConcurrencyLock)
                    {
                        if (_Current == null)
                        {
                            _Current = new T();
                        }
                    }
                }

                return _Current;
            }
            set
            {
                lock (_ConcurrencyLock)
                {
                    _Current = value;
                }
            }
        }
    }
    public class SerializationHelper : InterfacedSingletonBase<SerializationHelper,ISerializationHelper>, ISerializationHelper
    {
        #region XmlSerializer Cache

        private static readonly Dictionary<Type, XmlSerializer> Serializers = new Dictionary<Type, XmlSerializer>();
        private static readonly object SerializerLock = new object();

        private XmlSerializer GetXmlSerializer(Type type)
        {
            XmlSerializer serializer;

            lock (SerializerLock)
            {
                if (Serializers.ContainsKey(type))
                {
                    serializer = Serializers[type];
                }
                else
                {

                    if (Serializers.ContainsKey(type))
                        serializer = Serializers[type];
                    else
                    {
                        serializer = new XmlSerializer(type);
                        Serializers.Add(type, serializer);
                    }
                }
            }

            return serializer;
        }

        #endregion

        /// <summary>
        /// De-serializes a string into an instance of the object.
        /// </summary>
        /// <param name="data">The string containing the object data.</param>
        /// <param name="serializationType">The type of de-serialization to perform on the string.</param>
        /// <returns>The object created from the passed in string.</returns>
        public async Task<T> DeserializeAsync<T>(string data,
            SerializationTypes serializationType = SerializationTypes.Json) where T : class, new()
        {
            switch (serializationType)
            {
                case SerializationTypes.Xml:
                    return await Task.Factory.StartNew(() => FromXml<T>(data));

                case SerializationTypes.Json:
                    return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(data));

                default:
                    throw new ArgumentOutOfRangeException("serializationType");
            }
        }

        /// <summary>
        /// De-serializes a string into an instance of the object asynchronously.
        /// </summary>
        /// <param name="data">The string containing the object data.</param>
        /// <param name="serializationType">The type of de-serialization to perform on the string.</param>
        /// <returns>The object created from the passed in string.</returns>
        public T Deserialize<T>(string data, SerializationTypes serializationType = SerializationTypes.Json)
            where T : class, new()
        {
            switch (serializationType)
            {
                case SerializationTypes.Xml:
                    return FromXml<T>(data);

                case SerializationTypes.Json:
                    return JsonConvert.DeserializeObject<T>(data);

                default:
                    throw new ArgumentOutOfRangeException("serializationType");
            }
        }

        /// <summary>
        /// Trys to de-serializes a string into an instance of the object.
        /// </summary>
        /// <param name="data">The string containing the object data.</param>
        /// <param name="serializationType">The type of de-serialization to perform on the string.</param>
        /// <param name="result">The serialized object or null if the string can't be serialized.</param>
        /// <returns>True if the string can be serialized, otherwise false.</returns>
        public bool TryDeserialize<T>(string data, SerializationTypes serializationType, out T result)
            where T : class, new()
        {
            try
            {
                result = Deserialize<T>(data, serializationType);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }

        }


        /// <summary>
        /// The most basic serialization of an object into an XML string.
        /// </summary>
        /// <returns>An XML string representing the object.</returns>
        public string ToXml(object obj)
        {
            return Serialize(obj, SerializationTypes.Xml);
        }

        /// <summary>
        /// The most basic serialization of an object into an XML string asynchronously.
        /// </summary>
        /// <returns>An XML string representing the object.</returns>
        public async Task<string> ToXmlAsync(object obj)
        {
            return await Task.Factory.StartNew(() => ToXml(obj));
        }

        /// <summary>
        /// The most basic de-serialization of an XML string into an object.
        /// </summary>
        /// <param name="xml">The XML string containing the object data.</param>
        /// <returns>The object created from the passed in XML string.</returns>
        public T FromXml<T>(string xml) where T : class, new()
        {
            using (TextReader reader = new StringReader(xml))
            {
                var serializer = GetXmlSerializer(typeof (T));
                return (T) serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// The most basic de-serialization of an XML string into an object asynchronously.
        /// </summary>
        /// <param name="xml">The XML string containing the object data.</param>
        /// <returns>The object created from the passed in XML string.</returns>
        public async Task<T> FromXmlAsync<T>(string xml) where T : class, new()
        {
            return await Task.Factory.StartNew(() => FromXml<T>(xml));
        }

 
 

        /// <summary>
        /// Serializes the object into a string.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="serializationType">The type of serialization to perform on the object.</param>
        /// <returns>A string representing the object.</returns>
        public string Serialize(object obj, SerializationTypes serializationType)
        {
            switch (serializationType)
            {
                case SerializationTypes.Xml:
                    return SerializeToXml(obj);

                case SerializationTypes.Json:
                    return JsonConvert.SerializeObject(obj);

                default:
                    throw new ArgumentOutOfRangeException("serializationType");
            }
        }
        /// <summary>
        /// Serializes the object into a string asynchronously.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="serializationType">The type of serialization to perform on the object asynchronously.</param>
        /// <returns>A string representing the object.</returns>
        public async Task<string> SerializeAsync(object obj, SerializationTypes serializationType)
        {
            switch (serializationType)
            {
                case SerializationTypes.Xml:
                    return await Task.Factory.StartNew(() => SerializeToXml(obj));

                case SerializationTypes.Json:
                    return await Task.Factory.StartNew(() => JsonConvert.SerializeObject(obj));

                default:
                    throw new ArgumentOutOfRangeException("serializationType");
            }
        }

        /// <summary>
        /// Performs the Serialization of the object to an XML string.
        /// </summary>
        /// <returns>An XML string representing the object.</returns>
        private string SerializeToXml(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            using (var stringWriter = new StringWriter())
            {
                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = false,
                    NewLineHandling = NewLineHandling.None,
                    OmitXmlDeclaration = true
                };

                var nameSpace = new XmlSerializerNamespaces();
                nameSpace.Add(string.Empty, string.Empty);

                using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                {

                    var xmlSerializer = GetXmlSerializer(obj.GetType());
                    xmlSerializer.Serialize(xmlWriter, obj, nameSpace);

                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Performs the Serialization of the object to an XML string.
        /// </summary>
        /// <returns>An XML string representing the object.</returns>
        private async Task<string> SerializeToXmlAsync(object obj)
        {
            if (obj == null)
            {
                return await Task.Factory.StartNew(() => string.Empty);//(return ));
            }
            return await Task.Factory.StartNew(() =>
            {
                using (var stringWriter = new StringWriter())
                {
                    var xmlWriterSettings = new XmlWriterSettings
                    {
                        Indent = false,
                        NewLineHandling = NewLineHandling.None,
                        OmitXmlDeclaration = true
                    };

                    var nameSpace = new XmlSerializerNamespaces();
                    nameSpace.Add(string.Empty, string.Empty);

                    using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                    {

                        var xmlSerializer = GetXmlSerializer(obj.GetType());
                        xmlSerializer.Serialize(xmlWriter, obj, nameSpace);

                        return stringWriter.ToString();
                    }
                }
            });

        }
 
      
    }
}