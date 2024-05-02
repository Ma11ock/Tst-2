
using Godot;
using Quake.PlayerInput.ConsoleCommand;

namespace Quake;

public partial class SceneManager : Node3D
{
    public ConsoleRegistry Registry { get; } = new ConsoleRegistry();
}
