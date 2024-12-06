using System;

namespace ModDemo.Json.Converter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
public class JsonDerivedTypeAttribute : Attribute
{
    public string TypeId { get; }
    public Type DerivedType { get; }

    public JsonDerivedTypeAttribute(string typeId, Type derivedType)
    {
        TypeId = typeId;
        DerivedType = derivedType;
    }
}