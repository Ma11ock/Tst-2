using Godot;

namespace Quake.Player;

public partial class Player : QuakePlayer
{
    public override void _Process(double dt)
    {
        base._Process(dt);
        float delta = (float)dt;

        if (UpdateCamera)
        {
            UpdateCameraTransform();
            UpdateCamera = false;
        }

        float interpolationFraction = (float)Mathf.Clamp(Engine.GetPhysicsInterpolationFraction(), 0.0, 1.0);

        Camera.GlobalTransform = CameraGtPrevious.InterpolateWith(CameraGtCurrent, interpolationFraction);

        CameraTargetPosition = CameraTargetPosition.Lerp(Head.GlobalTransform.Origin,
                                                         (delta * Speed * STAIRS_FEELING_COEFFICIENT * CameraLerpCoefficient));

        if (IsOnFloor())
        {
            TimeInAir = 0;
            CameraLerpCoefficient = 1.0f;
            Camera.SetY(CameraTargetPosition.Y);
        }
        else
        {
            TimeInAir += delta;
            if (TimeInAir > 1.0f)
            {
                CameraLerpCoefficient = (float)Mathf.Clamp(CameraLerpCoefficient + delta, 2.0, 4.0);
            }
            else
            {
                CameraLerpCoefficient = 2.0f;
            }
            Camera.SetY(CameraTargetPosition.Y);
        }
    }


    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        switch (@event)
        {
            case InputEventMouseMotion motionEvent:
                if (Input.MouseMode != Input.MouseModeEnum.Captured) break;

                Body.RotateY(Mathf.DegToRad(-motionEvent.Relative.X * MouseSensitivity));
                Head.RotateX(Mathf.DegToRad(-motionEvent.Relative.Y * MouseSensitivity));
                Head.SetRotationX(Mathf.Clamp(Head.Rotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89)));
                break;
            case InputEventMouseButton buttonEvent:
                if (buttonEvent.IsActionPressed("ui_click") && Input.MouseMode != Input.MouseModeEnum.Captured)
                {
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
                break;
            case InputEventKey keyEvent:
                if (keyEvent.IsActionPressed("ui_activate_mouse") && Input.MouseMode != Input.MouseModeEnum.Visible)
                {
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                }
                else if (keyEvent.IsActionPressed("ui_fullscreen"))
                {
                    if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen)
                    {
                        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                    }
                    else
                    {
                        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                    }
                }
                break;
            default:
                break;
        }
    }
}
