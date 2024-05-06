using Godot;

namespace Quake.Player;

public struct UserCommand
{
    public Vector3 ViewAngles;

    public float MoveX;

    public float MoveY;

    public bool Jump;

    public UserCommand(Vector3 viewAngles, float moveX, float moveY, bool jump)
    {
        ViewAngles = viewAngles;
        MoveX = moveX;
        MoveY = moveY;
        Jump = jump;
    }
}
