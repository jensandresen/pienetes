using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonMessageSerializer()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                IgnoreNullValues = false,
                WriteIndented = true
            };
            
            _jsonSerializerOptions.Converters.Add(new QueuedManifestIdJsonConverter());
        }

        public string Serialize(object message)
        {
            return System.Text.Json.JsonSerializer.Serialize(message, _jsonSerializerOptions);
        }

        public T Deserialize<T>(string message)
        {
            return JsonSerializer.Deserialize<T>(message, _jsonSerializerOptions);
        }

        public object Deserialize(string message, Type resultType)
        {
            return JsonSerializer.Deserialize(message, resultType, _jsonSerializerOptions);
        }
    }
    
    
    
    public class QueuedManifestIdJsonConverter : JsonConverter<QueuedManifestId>
    {
        public override QueuedManifestId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return QueuedManifestId.Parse(value ?? "");
        }

        public override void Write(Utf8JsonWriter writer, QueuedManifestId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}