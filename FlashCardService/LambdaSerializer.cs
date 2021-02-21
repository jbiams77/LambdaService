using System;
using System.IO;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;
using FlashCardService;
using Alexa.NET.InSkillPricing.Responses;

namespace MySerializer
{
    /// <summary>
    /// Custom ILambdaSerializer implementation which uses Newtonsoft.Json 9.0.1
    /// for serialization.
    /// 
    /// <para>
    /// If the environment variable LAMBDA_NET_SERIALIZER_DEBUG is set to true the JSON coming
    /// in from Lambda and being sent back to Lambda will be logged.
    /// </para>
    /// </summary>
    public class JsonSerializer : ILambdaSerializer
    {
        private const string DEBUG_ENVIRONMENT_VARIABLE_NAME = "LAMBDA_NET_SERIALIZER_DEBUG";
        private Newtonsoft.Json.JsonSerializer serializer;
        private bool debug;

        /// <summary>
        /// Constructs instance of serializer. 
        /// </summary>
        /// <param name="customizeSerializerSettings">A callback to customize the serializer settings.</param>
        public JsonSerializer(Action<JsonSerializerSettings> customizeSerializerSettings)
            : this(customizeSerializerSettings, null)
        {
            debug = true;
            
        }

        /// <summary>
        /// Constructs instance of serializer. This constructor is usefull to 
        /// customize the serializer settings.
        /// </summary>
        /// <param name="customizeSerializerSettings">A callback to customize the serializer settings.</param>
        /// <param name="namingStrategy">The naming strategy to use. This parameter makes it possible to change the naming strategy to camel case for example. When not provided, it uses the default Newtonsoft.Json DefaultNamingStrategy.</param>
        public JsonSerializer(Action<JsonSerializerSettings> customizeSerializerSettings, NamingStrategy namingStrategy)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            customizeSerializerSettings(settings);


            // Set the contract resolver *after* the custom callback has been 
            // invoked. This makes sure that we always use the good resolver.
            var resolver = new AwsResolver();
            if (namingStrategy != null)
            {
                resolver.NamingStrategy = namingStrategy;
            };
            settings.ContractResolver = resolver;

            serializer = Newtonsoft.Json.JsonSerializer.Create(settings);

            if (string.Equals(Environment.GetEnvironmentVariable(DEBUG_ENVIRONMENT_VARIABLE_NAME), "true", StringComparison.OrdinalIgnoreCase))
            {
                this.debug = true;
            }
        }

        /// <summary>
        /// Constructs instance of serializer.
        /// </summary>
        public JsonSerializer()
            : this(customizeSerializerSettings: _ => { /* Nothing to customize by default. */ })
        {
        }

        /// <summary>
        /// Constructs instance of serializer using custom converters.
        /// </summary>
        public JsonSerializer(IEnumerable<JsonConverter> converters)
                : this()
        {
            if (converters != null)
            {
                foreach (var c in converters)
                {
                    serializer.Converters.Add(c);
                }
            }
        }

        /// <summary>
        /// Serializes a particular object to a stream.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="response">Object to serialize.</param>
        /// <param name="responseStream">Output stream.</param>
        public void Serialize<T>(T response, Stream responseStream)
        {
            try
            {
                if (debug)
                {
                    using (StringWriter debugWriter = new StringWriter())
                    {
                        serializer.Serialize(debugWriter, response);
                        Console.WriteLine($"Lambda Serialize {response.GetType().FullName}: {debugWriter.ToString()}");

                        StreamWriter writer = new StreamWriter(responseStream);
                        writer.Write(debugWriter.ToString());
                        writer.Flush();
                    }
                }
                else
                {
                    StreamWriter writer = new StreamWriter(responseStream);
                    serializer.Serialize(writer, response);
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                throw new JsonSerializerException($"Error converting the response object of type {typeof(T).FullName} from the Lambda function to JSON: {e.Message}", e);
            }
        }

        /// <summary>
        /// Deserializes a stream to a particular type.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to.</typeparam>
        /// <param name="requestStream">Stream to serialize.</param>
        /// <returns>Deserialized object from stream.</returns>
        public T Deserialize<T>(Stream requestStream)
        {
            try
            {
                TextReader reader;
                if (debug)
                {
                    var json = new StreamReader(requestStream).ReadToEnd();
                    Console.WriteLine($"Lambda Deserialize {typeof(T).FullName}: {json}");                    
                    reader = new StringReader(json);
                }
                else
                {
                    reader = new StreamReader(requestStream);                    
                }

                JsonReader jsonReader = new JsonTextReader(reader);         
                
                return serializer.Deserialize<T>(jsonReader);
            }
            catch (Exception e)
            {
                string message;
                var targetType = typeof(T);
                if (targetType == typeof(string))
                {
                    message = $"Error converting the Lambda event JSON payload to a string. JSON strings must be quoted, for example \"Hello World\" in order to be converted to a string: {e.Message}";
                }
                else
                {
                    message = $"Error converting the Lambda event JSON payload to type {targetType.FullName}: {e.Message}";
                }
                throw new JsonSerializerException(message, e);
            }
        }

        /// <summary>
        /// Exception thrown when errors occur serializing and deserializng JSON documents from the Lambda service
        /// </summary>
        public class JsonSerializerException : Exception
        {
            /// <summary>
            /// Constructs instances of JsonSerializerException
            /// </summary>
            /// <param name="message">Exception message</param>
            public JsonSerializerException(string message) : base(message) { }

            /// <summary>
            /// Constructs instances of JsonSerializerException
            /// </summary>
            /// <param name="message">Exception message</param>
            /// <param name="exception">Inner exception for the JsonSerializerException</param>
            public JsonSerializerException(string message, Exception exception) : base(message, exception) { }
        }

    }

    /// <summary>
    /// Custom JSON converter for handling special event cases.
    /// </summary>
    internal class JsonToMemoryStreamDataConverter : JsonConverter
    {
        private static readonly TypeInfo MEMORYSTREAM_TYPEINFO = typeof(MemoryStream).GetTypeInfo();

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return false; } }
        public override bool CanConvert(Type objectType)
        {
            return MEMORYSTREAM_TYPEINFO.IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, NewtonsoftJsonSerializer serializer)
        {
            var dataBase64 = reader.Value as string;
            return Common.Base64ToMemoryStream(dataBase64);
        }

        public override void WriteJson(JsonWriter writer, object value, NewtonsoftJsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Custom contract resolver for handling special event cases.
    /// </summary>
    internal class AwsResolver : DefaultContractResolver
    {
        private JsonToMemoryStreamDataConverter jsonToMemoryStreamDataConverter;
        private JsonNumberToDateTimeDataConverter jsonNumberToDateTimeDataConverter;
        private JsonToMemoryStreamListDataConverter jsonToMemoryStreamListDataConverter;

        JsonToMemoryStreamDataConverter StreamDataConverter
        {
            get
            {
                if (jsonToMemoryStreamDataConverter == null)
                {
                    jsonToMemoryStreamDataConverter = new JsonToMemoryStreamDataConverter();
                }

                return jsonToMemoryStreamDataConverter;
            }
        }

        JsonToMemoryStreamListDataConverter StreamListDataConverter
        {
            get
            {
                if (jsonToMemoryStreamListDataConverter == null)
                {
                    jsonToMemoryStreamListDataConverter = new JsonToMemoryStreamListDataConverter();
                }

                return jsonToMemoryStreamListDataConverter;
            }
        }

        JsonNumberToDateTimeDataConverter DateTimeConverter
        {
            get
            {
                if (jsonNumberToDateTimeDataConverter == null)
                {
                    jsonNumberToDateTimeDataConverter = new JsonNumberToDateTimeDataConverter();
                }

                return jsonNumberToDateTimeDataConverter;
            }
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
           
            // S3 events use non-standard key formatting for request IDs and need to be mapped to the correct properties
            if (type.FullName.Equals("Amazon.S3.Util.S3EventNotification+ResponseElementsEntity", StringComparison.Ordinal))
            {
                foreach (JsonProperty property in properties)
                {
                    if (property.PropertyName.Equals("XAmzRequestId", StringComparison.Ordinal))
                    {
                        property.PropertyName = "x-amz-request-id";
                    }
                    else if (property.PropertyName.Equals("XAmzId2", StringComparison.Ordinal))
                    {
                        property.PropertyName = "x-amz-id-2";
                    }
                }
            }
            else if (type.FullName.Equals("Amazon.Lambda.KinesisEvents.KinesisEvent+Record", StringComparison.Ordinal))
            {
                foreach (JsonProperty property in properties)
                {
                    if (property.PropertyName.Equals("Data", StringComparison.Ordinal))
                    {
                        property.MemberConverter = StreamDataConverter;
                    }
                    else if (property.PropertyName.Equals("ApproximateArrivalTimestamp", StringComparison.Ordinal))
                    {
                        property.MemberConverter = DateTimeConverter;
                    }
                }
            }
            else if (type.FullName.Equals("Amazon.DynamoDBv2.Model.StreamRecord", StringComparison.Ordinal))
            {
                foreach (JsonProperty property in properties)
                {
                    if (property.PropertyName.Equals("ApproximateCreationDateTime", StringComparison.Ordinal))
                    {
                        property.MemberConverter = DateTimeConverter;
                    }
                }
            }
            else if (type.FullName.Equals("Amazon.DynamoDBv2.Model.AttributeValue", StringComparison.Ordinal))
            {
                foreach (JsonProperty property in properties)
                {
                    if (property.PropertyName.Equals("B", StringComparison.Ordinal))
                    {
                        property.MemberConverter = StreamDataConverter;
                    }
                    else if (property.PropertyName.Equals("BS", StringComparison.Ordinal))
                    {
                        property.MemberConverter = StreamListDataConverter;
                    }
                }
            }
            else if (type.FullName.Equals("Amazon.Lambda.SQSEvents.SQSEvent+MessageAttribute", StringComparison.Ordinal))
            {
                foreach (JsonProperty property in properties)
                {
                    if (property.PropertyName.Equals("BinaryValue", StringComparison.Ordinal))
                    {
                        property.MemberConverter = StreamDataConverter;
                    }
                    else if (property.PropertyName.Equals("BinaryListValues", StringComparison.Ordinal))
                    {
                        property.MemberConverter = StreamListDataConverter;
                    }
                }
            }
            else if (type.FullName.StartsWith("Amazon.Lambda.CloudWatchEvents.")
                     && (type.GetTypeInfo().BaseType?.FullName?.StartsWith("Amazon.Lambda.CloudWatchEvents.CloudWatchEvent`",
                             StringComparison.Ordinal) ?? false))
            {
                foreach (JsonProperty property in properties)
                {
                    if (property.PropertyName.Equals("DetailType", StringComparison.Ordinal))
                    {
                        property.PropertyName = "detail-type";
                    }
                }
            }

            return properties;
        }
    }

    /// <summary>
    /// Custom JSON converter for handling special event cases.
    /// </summary>
    internal class JsonToMemoryStreamListDataConverter : JsonConverter
    {
        private static readonly TypeInfo MEMORYSTREAM_LIST_TYPEINFO = typeof(List<MemoryStream>).GetTypeInfo();

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return false; } }
        public override bool CanConvert(Type objectType)
        {
            return MEMORYSTREAM_LIST_TYPEINFO.IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, NewtonsoftJsonSerializer serializer)
        {
            var list = new List<MemoryStream>();
            if (reader.TokenType == JsonToken.StartArray)
            {
                do
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.String)
                    {
                        var dataBase64 = reader.Value as string;
                        var ms = Common.Base64ToMemoryStream(dataBase64);
                        list.Add(ms);
                    }
                } while (reader.TokenType != JsonToken.EndArray);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, NewtonsoftJsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
    /// <summary>
    /// Custom JSON converter for handling special event cases.
    /// </summary>
    internal class JsonNumberToDateTimeDataConverter : JsonConverter
    {
        private static readonly TypeInfo DATETIME_TYPEINFO = typeof(DateTime).GetTypeInfo();
        private static readonly DateTime EPOCH_DATETIME = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return false; } }
        public override bool CanConvert(Type objectType)
        {
            return DATETIME_TYPEINFO.IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, NewtonsoftJsonSerializer serializer)
        {
            double seconds;
            switch (reader.TokenType)
            {
                case JsonToken.Float:
                    seconds = (double)reader.Value;
                    break;
                case JsonToken.Integer:
                    seconds = (long)reader.Value;
                    break;
                default:
                    seconds = 0;
                    break;
            }

            var result = EPOCH_DATETIME.AddSeconds(seconds);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, NewtonsoftJsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Common logic.
    /// </summary>
    internal static class Common
    {
        public static MemoryStream Base64ToMemoryStream(string dataBase64)
        {
            var dataBytes = Convert.FromBase64String(dataBase64);
            MemoryStream stream = new MemoryStream(dataBytes);
            return stream;
        }
    }
}