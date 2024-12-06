using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Converter;

public class JsonPolymorphicConverter<TBase> : JsonConverter<TBase> where TBase : class
{
    private readonly string _typeProperty;
    private readonly Dictionary<string, Type> _typeMapping;

    public JsonPolymorphicConverter(string typeProperty, Dictionary<string, Type> typeMapping)
    {
        _typeProperty = typeProperty;
        _typeMapping = typeMapping;
    }

    public override TBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var element = jsonDoc.RootElement;

        // Get the type discriminator
        if (!element.TryGetProperty(_typeProperty, out var typeProperty))
        {
            throw new JsonException($"Missing '{_typeProperty}' property");
        }

        var typeId = typeProperty.GetString();
        if (typeId == null || !_typeMapping.TryGetValue(typeId, out var targetType))
        {
            throw new JsonException($"Invalid type identifier: {typeId}");
        }

        // Deserialize to the concrete type
        var result = JsonSerializer.Deserialize(element.GetRawText(), targetType, options);
        return result as TBase;
    }

    public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Find type mapping for the concrete type
        var concreteType = value.GetType();
        var typeId = _typeMapping.FirstOrDefault(x => x.Value == concreteType).Key;
        
        if (typeId == null)
        {
            throw new JsonException($"No type mapping found for {concreteType.Name}");
        }

        JsonSerializer.Serialize(writer, value, concreteType, options);
    }
}