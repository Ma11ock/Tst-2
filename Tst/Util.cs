using Godot;
using System.IO;

namespace Quake;

public static class Util
{
    public static Main GetMain(this Node node) => node?.GetNode<Main>("/root/Scenes")!;

    public static void AddChildDeffered(this Node node, params Variant[] args) => node?.CallDeferred("add_child", args);

    public static bool IsCaseInsensitiveFileSystem(string path)
        => (File.Exists(path.ToUpper()) && File.Exists(path.ToLower())) || (Directory.Exists(path.ToUpper()) && Directory.Exists(path.ToLower()));

    public static Vector3 SetX(this Node3D node, float x) => node.Position = node.Position with { X = x };

    public static Vector3 SetY(this Node3D node, float y) => node.Position = node.Position with { Y = y };

    public static Vector3 SetZ(this Node3D node, float z) => node.Position = node.Position with { Z = z };

    public static Vector3 DeltaX(this Vector3 v, float x) => v with { X = x };

    public static Vector3 DeltaY(this Vector3 v, float y) => v with { Y = y };

    public static Vector3 DeltaZ(this Vector3 v, float z) => v with { Z = z };

    public static Vector3 SetRotationX(this Node3D node, float x) => node.Rotation = node.Rotation with { X = x };

    public static Vector3 SetRoationY(this Node3D node, float y) => node.Rotation = node.Rotation with { Y = y };

    public static Vector3 SetRotationZ(this Node3D node, float z) => node.Rotation = node.Rotation with { Z = z };

    public static Transform3D DeltaOrigin(this Transform3D t, Vector3 v) => t with { Origin = v };
}
