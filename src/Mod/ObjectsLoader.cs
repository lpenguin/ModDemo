using System;
using System.IO;
using System.Linq;
using Godot;
using ModDemo.Game.Objects;
using ModDemo.Json.Common;
using ModDemo.Json.Common.Extensions;
using ModDemo.Json.Objects;
using ModDemo.Util;
using Vector3 = Godot.Vector3;

namespace ModDemo.Mod;

public static class ObjectsLoader
{
    private const string ObjectsFile = "objects.json";
    private const string ObjectsFolder = "objects/";

    public static ObjectsCollection Load(string modDirectory)
    {
        string file = GodotPath.Combine(modDirectory, ObjectsFile);
        ObjectsContainer container = ObjectsReader.LoadFromFile(file);
        ObjectsCollection collection = new ObjectsCollection();
        foreach (ObjectDefinition objectDefinition in container.Objects)
        {
            Node3D node = objectDefinition switch
            {
                PropDefinition propDef => ReadProp(propDef, modDirectory),
                VehicleDefinition vehicleDef => ReadVehicle(vehicleDef, modDirectory),
                SceneDefinition sceneDef => ReadScene(sceneDef, modDirectory),
                WeaponDefinition weaponDef => ReadWeapon(weaponDef, modDirectory),
                _ => throw new NotImplementedException()
            };

            collection.AddObject(objectDefinition.Id, node);
            if (objectDefinition.Script != null)
            {
                node.AddChild(new ScriptNode
                {
                    Script = ReadScript(modDirectory, objectDefinition.Script),
                });
            }
        }

        return collection;
    }

    private static string ReadScript(string modDirectory, string script)
    {
        return GodotFile.ReadAllText(GodotPath.Combine(modDirectory, ObjectsFolder, script));
    }

    private static Node3D ReadWeapon(WeaponDefinition weaponDef, string modDirectory)
    {
        
    
        // Load and add mesh
        Node3D mesh = LoadMesh(weaponDef.Mesh, modDirectory);
    
        // Handle physics if specified
        Node3D physicsNode = weaponDef.Physics.Type switch
        {
            PhysicsType.RigidBody => new RigidBody3D { Mass = weaponDef.Physics.Mass },
            PhysicsType.Static => new StaticBody3D(),
            _ => throw new NotImplementedException()
        };

        // Create appropriate shape based on collider type
        CollisionShape3D collisionShape = weaponDef.Physics.Collider switch
        {
            BoxColliderProperties boxColliderProperties => CreateBoxCollider(GetAabb(mesh), boxColliderProperties),
            MeshColliderProperties meshColliderProperties => CreateMeshCollider(meshColliderProperties, modDirectory),
            _ => throw new NotSupportedException($"Unsupported collider type: {weaponDef.Physics.Collider}")
        };
        collisionShape.Name = "CollisionShape";
        physicsNode.AddChild(collisionShape);
        physicsNode.AddChild(mesh);
        

        var weaponObject = GodotScript.AttachScript<WeaponObject>(physicsNode);

        // Set projectile properties
        weaponObject.ProjectileType = weaponDef.Projectile.Type;
        weaponObject.ProjectileDamage = weaponDef.Projectile.Damage;
        weaponObject.ProjectileOrigin = weaponDef.Projectile.Transform?.ToGodot() ?? Transform3D.Identity;
        weaponObject.NumProjectiles = weaponDef.NumProjectiles;
        weaponObject.ShootDelay = weaponDef.ShootDelay;
        if (weaponDef.Projectile is BulletProjectileProperties bulletProjectileProperties)
        {
            weaponObject.ProjectileColor = bulletProjectileProperties.Color.ToGodot();
            weaponObject.ProjectileSpread = bulletProjectileProperties.Spread;
        }


        return weaponObject;
    }

    private static Node3D ReadProp(PropDefinition propDef, string modDirectory)
    {
        Node3D result;
        Node3D mesh = LoadMesh(propDef.Mesh, modDirectory);
        
        if (propDef.Physics != null)
        {
            result = propDef.Physics.Type switch
            {
                PhysicsType.RigidBody => new RigidBody3D { Mass = propDef.Physics.Mass },
                PhysicsType.Static => new StaticBody3D(),
                _ => throw new NotImplementedException()
            };

            // Create appropriate shape based on collider type
            CollisionShape3D collisionShape = propDef.Physics.Collider switch
            {
                BoxColliderProperties boxColliderProperties => CreateBoxCollider(GetAabb(mesh), boxColliderProperties),
                MeshColliderProperties meshColliderProperties => CreateMeshCollider(meshColliderProperties, modDirectory),
                _ => throw new NotSupportedException($"Unsupported collider type: {propDef.Physics.Collider}")
            };
            collisionShape.Name = "CollisionShape";
            result.AddChild(collisionShape);
        }
        else
        {
            result = new Node3D();
        }
        result.AddChild(mesh);


        return result;
    }

    private static CollisionShape3D CreateBoxCollider(Aabb aabb, BoxColliderProperties properties)
    {
        var collisionShape = new CollisionShape3D();
    
        // Create box shape with specified size
        var boxShape = new BoxShape3D();
        if (properties.Size == null)
        {
            boxShape.Size = aabb.Size;
        }
        else
        {
            boxShape.Size = new Vector3(
                properties.Size.X,
                properties.Size.Y,
                properties.Size.Z
            );
        }
    
        collisionShape.Shape = boxShape;

        if (properties.Transform == null)
        {
            // Calculate position based on AABB and box size
            // AABB center + offset to match the box collider's center with the mesh's center

            collisionShape.Position = aabb.Position + aabb.Size / 2;
        }
        else
        {
            // Apply transform from properties
            if (properties.Transform.Position != null)
                collisionShape.Position = properties.Transform.Position.ToGodot();
         
            if (properties.Transform.Rotation != null)
                collisionShape.Rotation = properties.Transform.Rotation.ToGodot();
         
            if (properties.Transform.Scale != null)
                collisionShape.Scale = properties.Transform.Scale.ToGodot();
        }

        return collisionShape;
    }
    private static CollisionShape3D CreateMeshCollider(MeshColliderProperties properties, string modFolder)
    {
        var collisionShape = new CollisionShape3D();
    
        // Load the collision mesh
        string meshPath = GodotPath.Combine(modFolder, ObjectsFolder, properties.Mesh);
        var collisionMesh = ResourceLoader.Load<Mesh>(meshPath);
    
        if (collisionMesh == null)
        {
            throw new FileNotFoundException($"Collision mesh not found: {meshPath}");
        }
    
        // Create concave polygon shape from mesh
        var shape = new ConcavePolygonShape3D();
        shape.Data = collisionMesh.GetFaces(); // Get triangles from mesh
    
        collisionShape.Shape = shape;
    
        // Apply transform from properties
        if (properties.Transform != null)
        {
            if (properties.Transform.Position != null)
                collisionShape.Position = properties.Transform.Position.ToGodot();
         
            if (properties.Transform.Rotation != null)
                collisionShape.Rotation = properties.Transform.Rotation.ToGodot();
         
            if (properties.Transform.Scale != null)
                collisionShape.Scale = properties.Transform.Scale.ToGodot();
        }
    
        return collisionShape;
    }

    private static Node3D LoadMesh(MeshProperties meshProperties, string modDirectory)
    {
        RenderProperties renderProperties = meshProperties.Render;
        string path = GodotPath.Combine(modDirectory, ObjectsFolder, renderProperties.Mesh);
        var resource = ResourceLoader.Load(path);
        Node3D result;
        if (resource is Mesh mesh)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;

            if (renderProperties.Texture != null)
            {
                string texturePath = GodotPath.Combine(modDirectory, ObjectsFolder, renderProperties.Texture);
                var texture = ResourceLoader.Load<Texture2D>(texturePath);

                if (texture != null)
                {
                    var material = new StandardMaterial3D();
                    material.AlbedoTexture = texture;
                    meshInstance.MaterialOverride = material;
                }
            }

            result = meshInstance;
        }
        else if (resource is PackedScene packedScene)
        {
            result = packedScene.Instantiate<Node3D>();
        }
        else
        {
            throw new NotImplementedException();
        }

        if (meshProperties.Transform != null)
        {
            result.Transform = meshProperties.Transform.ToGodot();
        }

        return result;
    }
    private static VehicleObject ReadVehicle(VehicleDefinition vehicleDef, string modDirectory)
    {
        var vehicleObject = new VehicleObject();

        Node3D visualInstance = LoadMesh(vehicleDef.Mesh, modDirectory);
        vehicleObject.AddChild(visualInstance);
        
        // Set up vehicle properties
        vehicleObject.MaxEngineForce = vehicleDef.Vehicle.EngineForce;
        vehicleObject.MaxBrakeForce = vehicleDef.Vehicle.BrakeForce;
        vehicleObject.MaxSteeringAngle = vehicleDef.Vehicle.SteeringAngle;
        vehicleObject.WeaponSlots = vehicleDef.WeaponSlots.Select(v => v.ToGodot()).ToArray();

        CollisionShape3D collisionShape = vehicleDef.Physics.Collider switch
        {
            // TODO:
            BoxColliderProperties boxColliderProperties => CreateBoxCollider(GetAabb(visualInstance), boxColliderProperties),
            MeshColliderProperties meshColliderProperties => CreateMeshCollider(meshColliderProperties, modDirectory),
            _ => throw new NotSupportedException($"Unsupported collider type: {vehicleDef.Physics.Collider}")
        };
        vehicleObject.AddChild(collisionShape);

        // Set up physics properties
        if (vehicleDef.Physics != null)
        {
            vehicleObject.Mass = vehicleDef.Physics.Mass;
        }

        // Set up wheels
        foreach (var wheelDef in vehicleDef.Wheels)
        {
            var wheel = new VehicleWheel3D();
            wheel.UseAsTraction = wheelDef.UseAsTraction;
            wheel.UseAsSteering = wheelDef.UseAsSteering;

            wheel.Transform = wheelDef.Transform.ToGodot();
            wheel.AddChild(LoadMesh(wheelDef.Mesh, modDirectory));
            vehicleObject.AddChild(wheel);
        }

        return vehicleObject;
    }

    private static Node3D ReadScene(SceneDefinition sceneDef, string modDirectory)
    {
        string scenePath = GodotPath.Combine(modDirectory, "objects", sceneDef.File);
        var scene = ResourceLoader.Load<PackedScene>(scenePath);
        if (scene == null)
        {
            throw new FileNotFoundException($"Scene not found: {scenePath}");
        }
        return scene.Instantiate<Node3D>();
    }

    private static Aabb GetAabb(Node3D visualInstance)
    {
        if (visualInstance is MeshInstance3D { Mesh: not null } meshInstance)
        {
            return GetAabb(meshInstance);
        }
        
        Aabb combinedAabb = new Aabb();
        bool first = true;
        
        foreach (Node child in visualInstance.GetChildren())
        {
            if (child is not MeshInstance3D childMeshInstance || childMeshInstance.Mesh == null)
                continue;

            Aabb childAabb = GetAabb(childMeshInstance);
            
            if (first)
            {
                combinedAabb = childAabb;
                first = false;
            }
            else
            {
                combinedAabb = combinedAabb.Merge(childAabb);
            }
        }
        
        return combinedAabb;
    }

    private static Aabb GetAabb(MeshInstance3D meshInstance)
    {
        var aabb = meshInstance.Mesh.GetAabb();
        aabb.Size *= meshInstance.Scale;
        aabb.Position *= meshInstance.Scale;
        aabb.Position += meshInstance.Position;
        return aabb;
    }
}
