using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModDemo.Util;

namespace ModDemo.Json.Levels;

public static class LevelReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Level LoadFromFile(string filePath)
    {
        try
        {
            string jsonContent = GodotFile.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<Level>(jsonContent, Options);
            
            if (result == null)
                throw new JsonException("Failed to deserialize level");

            return result;
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            throw new Exception($"Failed to load level from {filePath}", ex);
        }
    }

    public static void SaveToFile(Level level, string filePath)
    {
        try
        {
            string jsonContent = JsonSerializer.Serialize(level, Options);
            GodotFile.WriteAllText(filePath, jsonContent);
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            throw new Exception($"Failed to save level to {filePath}", ex);
        }
    }

    public static bool Validate(Level level, out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(level.Id))
            errors.Add("Level ID is required");

        if (string.IsNullOrWhiteSpace(level.Name))
            errors.Add("Level name is required");

        if (level.Objects == null || level.Objects.Count == 0)
        {
            errors.Add("Level must contain at least one object");
            return false;
        }

        foreach (var obj in level.Objects)
        {
            if (obj == null)
            {
                errors.Add("Level object cannot be null");
                continue;
            }

            if (string.IsNullOrWhiteSpace(obj.ObjectId))
                errors.Add($"ObjectId is required for level object{(string.IsNullOrWhiteSpace(obj.Name) ? "" : $" '{obj.Name}'")}");

            if (obj.Transform == null)
            {
                errors.Add($"Transform is required for object '{obj.ObjectId}'");
            }
            else
            {
                if (obj.Transform.Position == null)
                {
                    errors.Add($"Position is required in transform for object '{obj.ObjectId}'");
                }
            }
        }

        return errors.Count == 0;
    }
}