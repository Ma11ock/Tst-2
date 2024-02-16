using Godot;

namespace Quake;

public partial class Player : CharacterBody3D
{
    public const double ACCELERATION_DEFAULT = 7.0;
    public const double ACCELERATION_AIR = 1.0;
    public const double SPEED_DEFAULT = 7.0;
    public const double SPEED_ON_STAIRS = 5.0;

    public const double MAX_VELOCITY_AIR = 0.6;
    public const double MAX_VELOCITY_GROUND = 8.0;
    public const double MAX_ACCELERATION = 10 * MAX_VELOCITY_GROUND;
    public const double STOP_SPEED = 2.0;
    public static readonly double JUMP_IMPULSE = Mathf.Sqrt(2.0 * 9.8 * 0.85) * 1.4;
    public const double SMOOTHNESS = 10.0;


    public const double STAIRS_FEELING_COEFFICIENT = 2.5;
    public const double WALL_MARGIN = 0.001;
    public const double STEP_DOWN_MARGIN = 0.01;
    public static readonly Vector3 STEP_HEIGHT_DEFAULT = new Vector3(0, 0.6F, 0);
    public const double STEP_MAX_SLOPE_DEGREE = 40.0;
    public const int STEP_CHECK_COUNT = 2;
    public const double SPEED_CLAMP_AFTER_JUMP_COEFFICIENT = 0.4;
    public const double SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT = 0.4;

    public Node3D Body { get; private set; }
    public Node3D Head { get; private set; }
    public Marker3D CameraTarget { get; private set; }
    public Camera3D Camera { get; private set; }
    public Vector3 HeadPosition { get; private set; }

    public float Speed { get; private set; }

    public float Gravity { get; private set; }

    public Vector3 MainVelocity { get; private set; } = Vector3.Zero;

    public Vector3 GravityDirection { get; private set; } = Vector3.Zero;

    public Vector3 Movement { get; private set; } = Vector3.Zero;

    public Vector3 StepCheckHeight { get; private set; } = STEP_HEIGHT_DEFAULT / STEP_CHECK_COUNT;
    public bool IsJumping = false;
    public bool IsInAir = false;

    public Vector3 HeadOffset = Vector3.Zero;
    public Vector3 CameraTargetPosition = Vector3.Zero;
    public double CameraLerpCoefficient = 1.0;
    public double TimeInAir = 0.0;
    public bool UpdateCamera = false;
    public Transform3D CameraGtPrevious;
    public Transform3D CameraGtCurrent;


    public double Friction = 4.0;

    public float MouseSensitivity = 0.2F;
    public Vector3 Direction = Vector3.Zero;
    public bool WishJump = false;

    public Vector2 CameraInput = Vector2.Zero;
    public Vector2 CameraRotationVelocity = Vector2.Zero;

    private struct StepResult
    {
        public Vector3 DiffPosition;
        public Vector3 Normal;
        public bool IsStepUp;

        public StepResult()
        {
            DiffPosition = Vector3.Zero;
            Normal = Vector3.Zero;
            IsStepUp = false;
        }
    }


    public override void _Ready()
    {
        base._Ready();

        // Onreadies
        Body = GetNode<Node3D>("Body");
        Head = Body.GetNode<Node3D>("Head");
        CameraTarget = Head.GetNode<Marker3D>("CameraMarker3D");
        Camera = CameraTarget.GetNode<Camera3D>("Camera3D");

        //
        Input.MouseMode = Input.MouseModeEnum.Captured;

        CameraTargetPosition = Camera.GlobalTransform.Origin;
        Camera.TopLevel = true;
        Camera.GlobalTransform = CameraTarget.GlobalTransform;

        CameraGtCurrent = CameraGtPrevious = CameraTarget.GlobalTransform;
    }

    public void UpdateCameraTransform()
    {
        CameraGtPrevious = CameraGtCurrent;
        CameraGtCurrent = CameraTarget.GlobalTransform;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (UpdateCamera)
        {
            UpdateCameraTransform();
            UpdateCamera = false;
        }

        double interpolationFraction = Mathf.Clamp(Engine.GetPhysicsInterpolationFraction(), 0.0, 1.0);

        Transform3D cameraXform = CameraGtPrevious.InterpolateWith(CameraGtCurrent, (float)interpolationFraction);
        Camera.GlobalTransform = cameraXform;

        Transform3D headXform = Head.GlobalTransform;

        CameraTargetPosition = CameraTargetPosition.Lerp(headXform.Origin,
                                                         (float)(delta * Speed * STAIRS_FEELING_COEFFICIENT * CameraLerpCoefficient));

        if (IsOnFloor())
        {
            TimeInAir = 0;
            CameraLerpCoefficient = 1.0;
            Camera.SetY(CameraTargetPosition.Y);
        }
        else
        {
            TimeInAir += delta;
            if (TimeInAir > 1.0)
            {
                CameraLerpCoefficient += delta;
                CameraLerpCoefficient = Mathf.Clamp(CameraLerpCoefficient, 2.0, 4.0);
            }
            else
            {
                CameraLerpCoefficient = 2.0;
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

                CameraInput = motionEvent.Relative;
                Body.RotateY(Mathf.DegToRad(-CameraInput.X * MouseSensitivity));
                Body.RotateX(Mathf.DegToRad(-CameraInput.Y * MouseSensitivity));
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
