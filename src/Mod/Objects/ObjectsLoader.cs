using System;
using System.IO;
using Godot;
using ModDemo.Json.Common.Extensions;
using ModDemo.Json.Objects;
using ModDemo.Util;
using Vector3 = Godot.Vector3;

namespace ModDemo.Mod.Objects;

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
                ModDemo.Json.Objects.PropObject propObject => ReadProp(propObject, modDirectory),
                ModDemo.Json.Objects.VehicleObject vehicleObject => ReadVehicle(vehicleObject, modDirectory),
                ModDemo.Json.Objects.SceneObject sceneObject => ReadScene(sceneObject, modDirectory),
                _ => throw new NotImplementedException()
            };

            collection.AddObject(objectDefinition.Id, node);
        }

        return collection;
    }

    private static Node3D ReadProp(PropObject objectDef, string modDirectory)
    {
        Node3D result;
        Node3D mesh = LoadMesh(objectDef.Mesh, modDirectory);
        
        if (objectDef.Physics != null)
        {
            result = objectDef.Physics.Type switch
            {
                PhysicsType.RigidBody => new RigidBody3D { Mass = objectDef.Physics.Mass },
                PhysicsType.Static => new StaticBody3D(),
                _ => throw new NotImplementedException()
            };

            // Create appropriate shape based on collider type
            CollisionShape3D collisionShape = objectDef.Physics.Collider switch
            {
                BoxColliderProperties boxColliderProperties => CreateBoxCollider(GetAabb(mesh), boxColliderProperties),
                MeshColliderProperties meshColliderProperties => CreateMeshCollider(meshColliderProperties),
                _ => throw new NotSupportedException($"Unsupported collider type: {objectDef.Physics.Collider}")
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
    private static CollisionShape3D CreateMeshCollider(MeshColliderProperties properties)
    {
        var collisionShape = new CollisionShape3D();
    
        // Load the collision mesh
        string meshPath = GodotPath.Combine("res://", properties.Mesh);
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
    private static VehicleObject ReadVehicle(ModDemo.Json.Objects.VehicleObject objectDef, string modDirectory)
    {
        var vehicleObject = new VehicleObject();

        Node3D visualInstance = LoadMesh(objectDef.Mesh, modDirectory);
        vehicleObject.AddChild(visualInstance);
        
        // Set up vehicle properties
        vehicleObject.MaxEngineForce = objectDef.Vehicle.EngineForce;
        vehicleObject.MaxBrakeForce = objectDef.Vehicle.BrakeForce;
        vehicleObject.MaxSteeringAngle = objectDef.Vehicle.SteeringAngle;

        CollisionShape3D collisionShape = objectDef.Physics.Collider switch
        {
            // TODO:
            BoxColliderProperties boxColliderProperties => CreateBoxCollider(GetAabb(visualInstance), boxColliderProperties),
            MeshColliderProperties meshColliderProperties => CreateMeshCollider(meshColliderProperties),
            _ => throw new NotSupportedException($"Unsupported collider type: {objectDef.Physics.Collider}")
        };
        vehicleObject.AddChild(collisionShape);

        // Set up physics properties
        if (objectDef.Physics != null)
        {
            vehicleObject.Mass = objectDef.Physics.Mass;
        }

        // Set up wheels
        foreach (var wheelDef in objectDef.Wheels)
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

    private static Node3D ReadScene(SceneObject objectDef, string modDirectory)
    {
        string scenePath = GodotPath.Combine(modDirectory, objectDef.File);
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
            var aabb = meshInstance.Mesh.GetAabb();
            aabb.Size *= meshInstance.Scale;
            return aabb;
        }
        
        Aabb combinedAabb = new Aabb();
        bool first = true;
        
        foreach (Node child in visualInstance.GetChildren())
        {
            if (child is not MeshInstance3D childMeshInstance || childMeshInstance.Mesh == null)
                continue;
                
            Aabb childAabb = childMeshInstance.Mesh.GetAabb();
            childAabb.Position *= childMeshInstance.Scale;
            childAabb.Position += childMeshInstance.Position;
            childAabb.Size *= childMeshInstance.Scale;
            
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
}
