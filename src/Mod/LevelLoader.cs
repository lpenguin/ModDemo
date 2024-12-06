using System;
using Godot;
using ModDemo.Game.Objects;
using ModDemo.Json.Common.Extensions;
using ModDemo.Json.Levels;


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

                    if (instance is VehicleObject vehicleObject)
                    {
                        if (levelObj.HasTag("player"))
                        {
                            vehicleObject.AddToGroup("vehicles");
                            vehicleObject.ControlledByPlayer = true;
                        }

                        for (int i = 0; i < vehicleObject.WeaponSlots.Length; i++)
                        {
                            string slotTag = $"slot{i + 1}";
                            if (levelObj.HasTag(slotTag))
                            {
                                string weaponId = levelObj.GetTagValue(slotTag);
                                if (objectsCollection.TryGetObject(weaponId, out Node3D? weaponNode))
                                {
                                    var weaponObject = (WeaponObject)weaponNode;
                                    weaponObject.Freeze = true;
                                    vehicleObject.SetWeapon(i, weaponObject);
                                }
                            }
                        }
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