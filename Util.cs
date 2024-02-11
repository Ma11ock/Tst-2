using Godot;
using System.IO;

namespace Quake;

public static class Util
{
    public static Main GetMain(this Node node) => node?.GetNode<Main>("/root/Scenes")!;

    public static void AddChildDeffered(this Node node, params Variant[] args) => node?.CallDeferred("add_child", args);

    public static bool IsCaseInsensitiveFileSystem(string path) => (File.Exists(path.ToUpper()) && File.Exists(path.ToLower())) || (Directory.Exists(path.ToUpper()) && Directory.Exists(path.ToLower()));
}
