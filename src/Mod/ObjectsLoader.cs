using System;
using System.IO;
using System.Linq;
using Godot;
using ModDemo.Editor;
using ModDemo.Game.Objects;
using ModDemo.Json.Common.Extensions;
using ModDemo.Json.Objects;
using ModDemo.Util;
using Vector3 = Godot.Vector3;

namespace ModDemo.Mod;

public class ObjectsLoader
{
    private readonly Mod _mod;

    public ObjectsLoader(Mod mod)
    {
        _mod = mod;
    }

    public ObjectsCollection LoadObjects()
    {
        ObjectsCollection collection = new();
        foreach (ObjectDefinition objectDefinition in _mod.ObjectDefinitions.Values)
        {
            Node3D node = objectDefinition switch
            {
                PropDefinition propDef => ReadProp(propDef),
                VehicleDefinition vehicleDef => ReadVehicle(vehicleDef),
                SceneDefinition sceneDef => ReadScene(sceneDef),
                WeaponDefinition weaponDef => ReadWeapon(weaponDef),
                EmptyDefinition => ReadEmpty(),
                _ => throw new NotImplementedException()
            };

            if (objectDefinition.Script != null)
            {
                node.AddChild(new ScriptNode
                {
                    Script = ReadScript(objectDefinition.Script),
                });
            }

            collection.AddObject(objectDefinition.Id, node);
        }

        return collection;
    }

    private static Node3D ReadEmpty()
    {
        return new Node3D();
    }

    private string ReadScript(string script)
    {
        return GodotFile.ReadAllText(_mod.GetResourcePath(script));
    }

    private Node3D ReadWeapon(WeaponDefinition weaponDef)
    {
        // Load and add mesh
        Node3D mesh = LoadMesh(weaponDef.Mesh);
    
        // Handle physics if specified
        Node3D physicsNode = weaponDef.Physics.Type switch
        {
            PhysicsType.RigidBody => new RigidBody3D { Mass = weaponDef.Physics.Mass },
            PhysicsType.Static => new StaticBody3D(),
            _ => throw new NotImplementedException()
        };

        // Create appropriate shape based on collider type
        physicsNode.AddChild(CreateCollisionShape(weaponDef.Physics, mesh));
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

    private Node3D ReadProp(PropDefinition propDef)
    {
        Node3D result;
        Node3D mesh = LoadMesh(propDef.Mesh);
        
        if (propDef.Physics != null)
        {
            result = propDef.Physics.Type switch
            {
                PhysicsType.RigidBody => new RigidBody3D { Mass = propDef.Physics.Mass },
                PhysicsType.Static => new StaticBody3D(),
                _ => throw new NotImplementedException()
            };

            // Create appropriate shape based on collider type
            var collisionShape = CreateCollisionShape(propDef.Physics, mesh);
            result.AddChild(collisionShape);
        }
        else
        {
            result = new Node3D();
        }
        result.AddChild(mesh);


        return result;
    }

    public CollisionShape3D CreateCollisionShape(PhysicsProperties physicsDef, Node3D mesh)
    {
        CollisionShape3D collisionShape = physicsDef.Collider switch
        {
            BoxColliderProperties boxColliderProperties => CreateBoxCollider(GetAabb(mesh), boxColliderProperties),
            MeshColliderProperties meshColliderProperties => CreateMeshCollider(meshColliderProperties),
            _ => throw new NotSupportedException($"Unsupported collider type: {physicsDef.Collider}")
        };
        collisionShape.Name = "CollisionShape";
        return collisionShape;
    }

    public Node3D LoadMesh(MeshProperties meshProperties)
    {
        RenderProperties renderProperties = meshProperties.Render;
        var resource = _mod.LoadResource(meshProperties.Render.Mesh);
        Node3D result;
        if (resource is Mesh mesh)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;

            if (renderProperties.Texture != null)
            {
                var texture = _mod.LoadResource<Texture2D>(renderProperties.Texture);

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
    private VehicleObject ReadVehicle(VehicleDefinition vehicleDef)
    {
        var vehicleObject = new VehicleObject();

        Node3D visualInstance = LoadMesh(vehicleDef.Mesh);
        vehicleObject.AddChild(visualInstance);
        
        // Set up vehicle properties
        vehicleObject.MaxEngineForce = vehicleDef.Vehicle.EngineForce;
        vehicleObject.MaxBrakeForce = vehicleDef.Vehicle.BrakeForce;
        vehicleObject.MaxSteeringAngle = vehicleDef.Vehicle.SteeringAngle;
        vehicleObject.WeaponSlots = vehicleDef.WeaponSlots.Select(v => v.ToGodot()).ToArray();

        vehicleObject.AddChild(CreateCollisionShape(vehicleDef.Physics, visualInstance));

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
            wheel.AddChild(LoadMesh(wheelDef.Mesh));
            vehicleObject.AddChild(wheel);
        }

        return vehicleObject;
    }

    private Node3D ReadScene(SceneDefinition sceneDef)
    {
        var scene = _mod.LoadResource<PackedScene>(sceneDef.File);
        if (scene == null)
        {
            throw new FileNotFoundException($"Scene not found: {sceneDef.File}");
        }
        return scene.Instantiate<Node3D>();
    }

    public static Aabb GetAabb(Node3D visualInstance)
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

    private CollisionShape3D CreateMeshCollider(MeshColliderProperties properties)
    {
        var collisionShape = new CollisionShape3D();
    
        // Load the collision mesh
        var collisionMesh = _mod.LoadResource<Mesh>(properties.Mesh);
    
        if (collisionMesh == null)
        {
            throw new FileNotFoundException($"Collision mesh not found: {properties.Mesh}");
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

    public static CollisionShape3D CreateBoxCollider(Aabb aabb, BoxColliderProperties properties)
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
    
    public LevelEditorObject LoadLevelEditorObject(ObjectDefinition definition)
    {
        MeshProperties? meshProperties = definition switch
        {
            PropDefinition prop => prop.Mesh,
            VehicleDefinition vehicle => vehicle.Mesh,
            WeaponDefinition weapon => weapon.Mesh,
            _ => null
        };

        var editorObject = new LevelEditorObject(definition.Id);
        if (meshProperties == null)
        {
            return editorObject;
        }

        var visualInstance = LoadMesh(meshProperties);  
        editorObject.AddChild(visualInstance);
        PhysicsProperties? physicsProperties = definition switch
        {
            PropDefinition prop => prop.Physics,
            VehicleDefinition vehicle => vehicle.Physics,
            WeaponDefinition weapon => weapon.Physics,
            _ => null
        };

        if (physicsProperties == null)
        {
            return editorObject;
        }

        var collider = CreateCollisionShape(physicsProperties, visualInstance);
        var area3d = new Area3D();
        area3d.AddChild(collider);
        editorObject.AddChild(area3d);
        return editorObject;
    }
}
