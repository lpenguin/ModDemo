using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Converter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = false)]
public class JsonDerivedTypeConverterAttribute : JsonConverterAttribute
{
    private readonly string _typeProperty;

    public JsonDerivedTypeConverterAttribute(string typeProperty)
    {
        _typeProperty = typeProperty;
    }

    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        var typeMapping = new Dictionary<string, Type>();
        
        // Look for JsonDerivedType attributes
        var derivedTypeAttributes = typeToConvert.GetCustomAttributes(typeof(JsonDerivedTypeAttribute), true)
            .Cast<JsonDerivedTypeAttribute>();
            
        // Add all derived type mappings
        foreach (var attribute in derivedTypeAttributes)
        {
            typeMapping[attribute.TypeId] = attribute.DerivedType;
        }

        var converterType = typeof(JsonPolymorphicConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, _typeProperty, typeMapping);
    }
}