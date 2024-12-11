using Godot;

namespace ModDemo.Editor;

public class GizmoRayTester
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction.Normalized();
        }
    }

    public struct RayHit
    {
        public enum HitType
        {
            None,
            Axis,
            Plane
        }

        public HitType Type;
        public int ElementIndex; // 0=X, 1=Y, 2=Z for axes, same for planes
        public float Distance;
        public Vector3 Point;

        public static RayHit None => new() { Type = HitType.None, Distance = float.MaxValue };
    }

    private const float BaseAxisThreshold = 0.1f;

    private static (Vector3 point1, Vector3 point2, float distance) ClosestPointsBetweenLines(
        Vector3 line1Origin, Vector3 line1Dir,
        Vector3 line2Origin, Vector3 line2Dir)
    {
        line1Dir = line1Dir.Normalized();
        line2Dir = line2Dir.Normalized();

        // If lines are parallel, return arbitrary points
        if (Mathf.Abs(line1Dir.Dot(line2Dir)) > 0.999f)
        {
            return (line1Origin, line2Origin, line1Origin.DistanceTo(line2Origin));
        }

        var w0 = line1Origin - line2Origin;
        var a = line1Dir.Dot(line1Dir);
        var b = line1Dir.Dot(line2Dir);
        var c = line2Dir.Dot(line2Dir);
        var d = line1Dir.Dot(w0);
        var e = line2Dir.Dot(w0);

        var denominator = a * c - b * b;
        
        var s = (b * e - c * d) / denominator;
        var t = (a * e - b * d) / denominator;

        var point1 = line1Origin + line1Dir * s;
        var point2 = line2Origin + line2Dir * t;

        return (point1, point2, point1.DistanceTo(point2));
    }

    public RayHit TestAxis(Ray ray, Vector3 axisOrigin, Vector3 axisDirection, int axisIndex)
    {
        var (rayPoint, axisPoint, distance) = ClosestPointsBetweenLines(
            ray.Origin, ray.Direction,
            axisOrigin, axisDirection
        );

        // Calculate scaled threshold based on distance from ray origin to axis origin
        float distanceToGizmo = axisOrigin.DistanceTo(ray.Origin);
        float scaledThreshold = BaseAxisThreshold * (distanceToGizmo * 0.1f);
        
        // Scale clamping to prevent threshold from becoming too large or too small
        scaledThreshold = Mathf.Clamp(scaledThreshold, BaseAxisThreshold * 0.5f, BaseAxisThreshold * 5.0f);

        // Use scaled length from GizmoTool
        float scaledAxisLength = GizmoTool.BaseAxisLength * (distanceToGizmo * 0.1f);
        scaledAxisLength = Mathf.Clamp(scaledAxisLength, 0.5f, 5.0f);

        var axisOffset = (axisPoint - axisOrigin).Length();
        if (distance <= scaledThreshold && axisOffset <= scaledAxisLength)
        {
            return new RayHit
            {
                Type = RayHit.HitType.Axis,
                ElementIndex = axisIndex,
                Distance = (rayPoint - ray.Origin).Length(),
                Point = axisPoint
            };
        }

        return RayHit.None;
    }

    public RayHit TestPlane(Ray ray, Vector3 planeOrigin, Vector3 planeNormal, int planeIndex)
    {
        var denominator = planeNormal.Dot(ray.Direction);
        
        if (Mathf.Abs(denominator) < 1e-6f)
        {
            return RayHit.None;
        }

        var t = (planeNormal.Dot(planeOrigin - ray.Origin)) / denominator;
        
        if (t < 0)
        {
            return RayHit.None;
        }

        var hitPoint = ray.Origin + ray.Direction * t;
        var toHit = hitPoint - planeOrigin;

        var u = planeNormal.Cross(Vector3.Up);
        if (u.LengthSquared() < 1e-6f)
        {
            u = planeNormal.Cross(Vector3.Right);
        }
        u = u.Normalized();
        var v = planeNormal.Cross(u).Normalized();

        var localX = toHit.Dot(u);
        var localY = toHit.Dot(v);

        // Scale the plane size based on distance
        float distanceToGizmo = planeOrigin.DistanceTo(ray.Origin);
        float scaledPlaneSize = GizmoTool.BasePlaneSize * (distanceToGizmo * 0.1f);
        scaledPlaneSize = Mathf.Clamp(scaledPlaneSize, 0.4f, 4.0f);

        if (Mathf.Abs(localX) <= scaledPlaneSize * 0.5f && Mathf.Abs(localY) <= scaledPlaneSize * 0.5f)
        {
            return new RayHit
            {
                Type = RayHit.HitType.Plane,
                ElementIndex = planeIndex,
                Distance = t,
                Point = hitPoint
            };
        }

        return RayHit.None;
    }

    public RayHit TestGizmo(Ray ray, Transform3D gizmoTransform)
    {
        var bestHit = RayHit.None;

        var hits = new[]
        {
            TestAxis(ray, gizmoTransform.Origin, gizmoTransform.Basis.X, 0),
            TestAxis(ray, gizmoTransform.Origin, gizmoTransform.Basis.Y, 1),
            TestAxis(ray, gizmoTransform.Origin, gizmoTransform.Basis.Z, 2),
            TestPlane(ray, gizmoTransform.Origin, gizmoTransform.Basis.Z, 0),
            TestPlane(ray, gizmoTransform.Origin, gizmoTransform.Basis.Y, 1),
            TestPlane(ray, gizmoTransform.Origin, gizmoTransform.Basis.X, 2)
        };

        foreach (var hit in hits)
        {
            if (hit.Type != RayHit.HitType.None && hit.Distance < bestHit.Distance)
            {
                bestHit = hit;
            }
        }

        return bestHit;
    }

    public static Ray CreateRayFromCamera(Camera3D camera, Vector2 mousePos)
    {
        return new Ray(
            camera.ProjectRayOrigin(mousePos),
            camera.ProjectRayNormal(mousePos)
        );
    }
}