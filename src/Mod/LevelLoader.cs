using System;
using System.Linq;
using Godot;
using ModDemo.Json.Common.Extensions;
using ModDemo.Json.Levels;
using ModDemo.Mod.Objects;


namespace ModDemo.Mod;

public static class LevelLoader
{
    public static Node3D CreateScene(Level level, ObjectsCollection objectsCollection)
    {
        try
        {
            // Create the root node for our level
            var levelNode = new Node3D
            {
                Name = level.Name
            };

            // Validate level before proceeding
            if (!LevelReader.Validate(level, out var errors))
            {
                var errorMsg = string.Join("\n", errors);
                GD.PrintErr($"Level validation failed:\n{errorMsg}");
                return levelNode;
            }

            // Create object instances
            foreach (var levelObj in level.Objects)
            {
                try
                {
                    // Get the object template
                    if (!objectsCollection.TryGetObject(levelObj.ObjectId, out var objectNode))
                    {
                        GD.PrintErr($"Object not found: {levelObj.ObjectId}");
                        continue;
                    }

                    // Create a new instance
                    var instance = (Node3D)objectNode.Duplicate();
                    
                    instance.Name = levelObj.Name;
                    instance.Transform = levelObj.Transform.ToGodot();

                    if (instance is VehicleObject vehicleObject && levelObj.Tags.Contains("player"))
                    {
                        vehicleObject.ControlledByPlayer = true;
                    }
                    // Add to level
                    levelNode.AddChild(instance);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to create object instance {levelObj.ObjectId}: {e.Message}");
                }
            }

            return levelNode;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create level scene: {e.Message}");
            return new Node3D();
        }
    }
}