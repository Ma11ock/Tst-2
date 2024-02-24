using Godot;
using Quake.Network;

namespace Quake;
internal partial class SceneManagerClient : SceneManager
{
    public void RecvWorldState(Packet packet)
    {
    }

    public override void _Ready()
    {
        base._Ready();

        this.AddChildDeffered(SceneManagerServer.TestStairsScene.Instantiate());
    }
}
