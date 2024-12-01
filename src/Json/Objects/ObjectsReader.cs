using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModDemo.Util;
using FileAccess = Godot.FileAccess;

namespace ModDemo.Json.Objects;

public static class ObjectsReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static ObjectsContainer LoadFromFile(string filePath)
    {
        try
        {
            string jsonContent = GodotFile.ReadAllText(filePath);
            
            var result = JsonSerializer.Deserialize<ObjectsContainer>(jsonContent, Options);
            
            if (result == null)
                throw new JsonException("Failed to deserialize objects container");

            return result;
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            throw new Exception($"Failed to load objects from {filePath}", ex);
        }
    }

    public static void SaveToFile(this ObjectsContainer container, string filePath)
    {
        try
        {
            string jsonContent = JsonSerializer.Serialize(container, Options);
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            file.StoreString(jsonContent);
            file.Close();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            throw new Exception($"Failed to save objects to {filePath}", ex);
        }
    }
}