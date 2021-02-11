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
            _jsonSerializerOptions.Converters.Add(new ValueObjectConverter<ServiceId>(obj => obj.ToString(), ServiceId.Create));
            _jsonSerializerOptions.Converters.Add(new ValueObjectConverter<ServiceImage>(obj => obj.ToString(), ServiceImage.Parse));
            _jsonSerializerOptions.Converters.Add(new ValueObjectConverter<ServiceSecret>(obj => obj.ToString(), ServiceSecret.Parse));
            _jsonSerializerOptions.Converters.Add(new ValueObjectConverter<ServicePortMapping>(obj => obj.ToString(), ServicePortMapping.Parse));
            _jsonSerializerOptions.Converters.Add(new ValueObjectConverter<ServiceEnvironmentVariable>(obj => obj.ToString(), ServiceEnvironmentVariable.Parse));
        }

        
        public string Serialize(object message)
        {
            return JsonSerializer.Serialize(message, _jsonSerializerOptions);
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
        public override QueuedManifestId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return QueuedManifestId.Parse(value ?? "");
        }

        public override void Write(Utf8JsonWriter writer, QueuedManifestId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    
    public class ValueObjectConverter<TValueObject> : JsonConverter<TValueObject> where TValueObject : ValueObject
    {
        private readonly Func<TValueObject, string> _serialize;
        private readonly Func<string, TValueObject> _deserialize;

        public ValueObjectConverter(Func<TValueObject, string> serialize, Func<string, TValueObject> deserialize)
        {
            _serialize = serialize;
            _deserialize = deserialize;
        }
        
        public override TValueObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value= reader.GetString();
            return _deserialize(value ?? "");
        }

        public override void Write(Utf8JsonWriter writer, TValueObject value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(_serialize(value));
        }
    }
}