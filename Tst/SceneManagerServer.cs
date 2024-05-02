using Godot;

namespace Quake;

public partial class SceneManagerServer : SceneManager
{
    // TODO remove
    public static readonly PackedScene TestStairsScene = ResourceLoader.Load<PackedScene>(@"res://TestStairs.tscn");
    public static readonly PackedScene PlayerCharacter = ResourceLoader.Load<PackedScene>(@"res://Player/Player.tscn");

    public Node3D PlayerObjects;

    public override void _Ready()
    {
        base._Ready();

        this.AddChildDeffered(TestStairsScene.Instantiate());

        PlayerObjects = GetNode<Node3D>("PlayerObjects");
    }

    public void AddPlayer(int id)
    {
        // PlayerObjects.AddChild();
    }
}
