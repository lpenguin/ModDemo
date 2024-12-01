using Godot;

namespace ModDemo.Json.Common.Extensions;

public static class TransformExt
{
    public static Transform3D ToGodot(this Transform transform)
    {
        var basis = Basis.Identity;
        if (transform.Rotation != null)
            basis = new Basis(Quaternion.FromEuler(transform.Rotation.ToGodot() * Mathf.Pi / 180f));
        if (transform.Scale != null)
            basis = basis.Scaled(transform.Scale.ToGodot());
            
        return new Transform3D(basis, transform.Position?.ToGodot() ?? Godot.Vector3.Zero);
    }
}
