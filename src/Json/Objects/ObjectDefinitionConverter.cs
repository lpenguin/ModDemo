﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class ObjectDefinitionConverter : JsonConverter<ObjectDefinition>
{
    public override ObjectDefinition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;
        string type = root.GetProperty("type").GetString();

        return type switch
        {
            "prop" => JsonSerializer.Deserialize<PropDefinition>(root.GetRawText(), options),
            "vehicle" => JsonSerializer.Deserialize<VehicleDefinition>(root.GetRawText(), options),
            "scene" => JsonSerializer.Deserialize<SceneDefinition>(root.GetRawText(), options),
            "weapon" => JsonSerializer.Deserialize<WeaponDefinition>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown object type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ObjectDefinition value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
