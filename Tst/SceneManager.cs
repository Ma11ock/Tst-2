
using Godot;

namespace Quake;

public abstract partial class SceneManager : Node3D
{
    public ConsoleRegistry Registry { get; } = new ConsoleRegistry();
}
