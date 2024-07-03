using Godot;

namespace Quake.Player;

public partial class QuakePlayer : CharacterBody3D
{
    public const float ACCELERATION_DEFAULT = 7.0f;
    public const float ACCELERATION_AIR = 1.0f;
    public const float SPEED_DEFAULT = 7.0f;
    public const float SPEED_ON_STAIRS = 5.0f;

    public const float MAX_VELOCITY_AIR = 0.6f;
    public const float MAX_VELOCITY_GROUND = 8.0f;
    public const float MAX_ACCELERATION = 10 * MAX_VELOCITY_GROUND;
    public const float STOP_SPEED = 2.0f;
    public static readonly float JUMP_IMPULSE = (float)(Mathf.Sqrt(2.0 * 9.8 * 0.85) * 1.4);
    public const float SMOOTHNESS = 10.0f;


    public const float STAIRS_FEELING_COEFFICIENT = 2.5f;
    public const float WALL_MARGIN = 0.001f;
    public const float STEP_DOWN_MARGIN = 0.01f;
    public static readonly Vector3 STEP_HEIGHT_DEFAULT = new Vector3(0, 0.6F, 0);
    public const float STEP_MAX_SLOPE_DEGREE = 40.0f;
    public const int STEP_CHECK_COUNT = 2;
    public const float SPEED_CLAMP_AFTER_JUMP_COEFFICIENT = 0.4f;
    public const float SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT = 0.4f;

    public const float GRAVITY_DEFAULT = 15.34f;

    public float Speed { get; private set; } = SPEED_DEFAULT;

    public float Gravity { get; private set; } = GRAVITY_DEFAULT;

    public Vector3 MainVelocity { get; private set; } = Vector3.Zero;

    public Vector3 GravityDirection { get; private set; } = Vector3.Zero;

    public Vector3 Movement { get; private set; } = Vector3.Zero;

    public Vector3 StepCheckHeight { get; private set; } = STEP_HEIGHT_DEFAULT / STEP_CHECK_COUNT;
    public bool IsJumping = false;
    public bool IsInAir = false;

    public Vector3 HeadOffset = Vector3.Zero;
    public Vector3 CameraTargetPosition = Vector3.Zero;
    public float CameraLerpCoefficient = 1.0f;
    public float TimeInAir = 0.0f;
    public bool UpdateCamera = false;
    public Transform3D CameraGtPrevious;
    public Transform3D CameraGtCurrent;


    public float Friction = 4.0f;

    public float MouseSensitivity = 0.2F;
    public Vector3 Direction = Vector3.Zero;
    public bool WishJump = false;

    public Vector2 CameraRotationVelocity = Vector2.Zero;

    public Node3D Body { get; private set; }
    public Node3D Head { get; private set; }
    public Camera3D Camera { get; private set; }
    public Marker3D CameraTarget { get; private set; }

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

        public StepResult(Vector3 diffPosition, Vector3 normal, bool isStepUp)
        {
            DiffPosition = diffPosition;
            Normal = normal;
            IsStepUp = isStepUp;
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

        // Basic setup
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

    public override void _PhysicsProcess(double dt)
    {
        base._PhysicsProcess(dt);
        float delta = (float)dt;


        UpdateCamera = true;

        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        Direction = (Body.GlobalTransform.Basis * new Vector3(input.X, 0.0f, input.Y)).Normalized();

        WishJump = Input.IsActionJustPressed("move_jump");

        if (IsOnFloor())
        {
            IsJumping = false;
            IsInAir = false;
            if (WishJump)
            {
                IsJumping = true;
                WishJump = false;
                MainVelocity = UpdateVelocityAir(Direction, delta);
                GravityDirection = Vector3.Up * JUMP_IMPULSE;
            }
            else
            {
                MainVelocity = UpdateVelocityGround(Direction, delta);
                GravityDirection = Vector3.Zero;
            }
        }
        else
        {
            IsInAir = true;
            MainVelocity = UpdateVelocityAir(Direction, delta);
            GravityDirection += Vector3.Down * Gravity * delta;
        }

        StepResult? maybeStepResult = StepCheck(IsJumping, delta);
        if (maybeStepResult is StepResult stepResult)
        {
            if (stepResult.IsStepUp)
            {
                if (IsInAir)
                {
                    MainVelocity *= SPEED_CLAMP_AFTER_JUMP_COEFFICIENT;
                    GravityDirection *= SPEED_CLAMP_AFTER_JUMP_COEFFICIENT;
                }
                else if (Direction.Dot(stepResult.Normal) > 0.0f)
                {
                    GlobalTransform = GlobalTransform.DeltaOrigin(GlobalTransform.Origin + MainVelocity * delta);
                    MainVelocity *= SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT;
                    GravityDirection *= SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT;
                }
            }
            GlobalTransform = GlobalTransform.DeltaOrigin(GlobalTransform.Origin + stepResult.DiffPosition);
            HeadOffset = stepResult.DiffPosition;
            Speed = SPEED_ON_STAIRS;
        }
        else
        {
            HeadOffset = HeadOffset.Lerp(Vector3.Zero, delta * Speed * STAIRS_FEELING_COEFFICIENT);

            if (Mathf.Abs(HeadOffset.Y) <= 0.01f)
            {
                Speed = SPEED_DEFAULT;
            }
        }


        MainVelocity = MainVelocity.DeltaY(0.0f);
        Movement = MainVelocity + GravityDirection;
        Velocity = Movement;
        MaxSlides = 6;
        MoveAndSlide();

        if (IsJumping)
        {
            IsJumping = false;
            IsInAir = true;
        }
    }


    private Vector3 Accelerate(Vector3 wishDir, float maxVelocity, float delta)
    {
        float vy = Velocity.Y;
        Velocity = Velocity.DeltaY(0.0f);
        float currentSpeed = Velocity.Dot(wishDir);
        float add_speed = Mathf.Clamp(maxVelocity - currentSpeed, 0.0f, MAX_ACCELERATION * delta);

        return (Velocity + add_speed * wishDir) + new Vector3(0, vy, 0);
    }

    private Vector3 UpdateVelocityAir(Vector3 wishDir, float delta)
        => Accelerate(wishDir, MAX_VELOCITY_AIR, delta);


    private Vector3 UpdateVelocityGround(Vector3 wishDir, float delta)
    {
        float speed = Velocity.Length();

        if (speed != 0.0f)
        {
            float control = Mathf.Max(STOP_SPEED, speed);
            float drop = control * Friction * delta;

            // Scale the velocity based off of friction.
            Velocity *= Mathf.Max(speed - drop, 0.0f) / speed;
        }

        return Accelerate(wishDir, MAX_VELOCITY_GROUND, delta);
    }

    private StepResult? StepCheck(bool isJumping, float delta)
    {
        bool isStep = false;
        Rid rid = GetRid();
        StepResult outResult = new StepResult();

        if (GravityDirection.Y >= 0)
        {
            for (int i = 0; i < STEP_CHECK_COUNT; i++)
            {
                PhysicsTestMotionResult3D testMotionResult = new PhysicsTestMotionResult3D();
                Vector3 stepHeight = STEP_HEIGHT_DEFAULT - i * StepCheckHeight;
                Transform3D transform = GlobalTransform;
                Vector3 motion = stepHeight;
                PhysicsTestMotionParameters3D testMotionParams = new PhysicsTestMotionParameters3D();
                testMotionParams.From = transform;
                testMotionParams.Motion = motion;

                bool isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                if (isPlayerCollided && (testMotionResult.GetCollisionNormal().Y < 0.0f))
                {
                    continue;
                }

                transform.Origin += stepHeight;
                motion = MainVelocity * delta;
                testMotionParams.From = transform;
                testMotionParams.Motion = motion;

                isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                if (!isPlayerCollided)
                {
                    transform.Origin += motion;
                    motion = -stepHeight;
                    testMotionParams.From = transform;
                    testMotionParams.Motion = motion;

                    isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                    if (isPlayerCollided &&
                        (testMotionResult.GetCollisionNormal().AngleTo(Vector3.Up) <= Mathf.DegToRad(STEP_MAX_SLOPE_DEGREE)))
                    {
                        isStep = true;
                        outResult = new StepResult(-testMotionResult.GetRemainder(), testMotionResult.GetCollisionNormal(), true);
                        break;
                    }
                }
                else
                {
                    Vector3 wallCollisionNormal = testMotionResult.GetCollisionNormal();
                    transform.Origin += wallCollisionNormal * WALL_MARGIN;
                    motion = (MainVelocity * delta).Slide(wallCollisionNormal);
                    testMotionParams.From = transform;
                    testMotionParams.Motion = motion;

                    isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                    if (!isPlayerCollided)
                    {
                        transform.Origin += motion;
                        motion = -stepHeight;
                        testMotionParams.From = transform;
                        testMotionParams.Motion = motion;

                        isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                        if (isPlayerCollided &&
                            (testMotionResult.GetCollisionNormal().AngleTo(Vector3.Up) <= Mathf.DegToRad(STEP_MAX_SLOPE_DEGREE)))
                        {
                            isStep = true;
                            outResult = new StepResult(-testMotionResult.GetRemainder(), testMotionResult.GetCollisionNormal(), true);
                        }
                    }
                }
            }
        }

        if (!isJumping && !isStep && IsOnFloor())
        {
            outResult = outResult with { IsStepUp = false };
            PhysicsTestMotionResult3D testMotionResult = new PhysicsTestMotionResult3D();
            Transform3D transform = GlobalTransform;
            Vector3 motion = MainVelocity * delta;
            PhysicsTestMotionParameters3D testMotionParams = new PhysicsTestMotionParameters3D();
            testMotionParams.From = transform;
            testMotionParams.Motion = motion;
            testMotionParams.RecoveryAsCollision = true;


            bool isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);
            if (!isPlayerCollided)
            {
                transform.Origin += motion;
                motion = -STEP_HEIGHT_DEFAULT;
                testMotionParams.From = transform;
                testMotionParams.Motion = motion;

                isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                if(isPlayerCollided &&
                   (testMotionResult.GetTravel().Y < -STEP_DOWN_MARGIN) &&
                   (testMotionResult.GetCollisionNormal().AngleTo(Vector3.Up) <= Mathf.DegToRad(STEP_MAX_SLOPE_DEGREE)))
                {
                    isStep = true;
                    outResult = new StepResult(testMotionResult.GetTravel(), testMotionResult.GetCollisionNormal(), false);
                }
            }
            else if (Mathf.IsZeroApprox(testMotionResult.GetCollisionNormal().Y))
            {
                Vector3 wallCollisionNormal = testMotionResult.GetCollisionNormal();
                transform.Origin += wallCollisionNormal * WALL_MARGIN;
                motion = (MainVelocity * delta).Slide(wallCollisionNormal);
                testMotionParams.From = transform;
                testMotionParams.Motion = motion;

                isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                if (!isPlayerCollided)
                {
                    transform.Origin += motion;
                    motion = -STEP_HEIGHT_DEFAULT;
                    testMotionParams.From = transform;
                    testMotionParams.Motion = motion;

                    isPlayerCollided = PhysicsServer3D.BodyTestMotion(rid, testMotionParams, testMotionResult);

                    if(isPlayerCollided &&
                       (testMotionResult.GetTravel().Y < -STEP_DOWN_MARGIN) &&
                       (testMotionResult.GetCollisionNormal().AngleTo(Vector3.Up) <= Mathf.DegToRad(STEP_MAX_SLOPE_DEGREE)))
                    {
                        outResult = new StepResult(testMotionResult.GetTravel(), testMotionResult.GetCollisionNormal(), true);
                    }
                }
            }
        }

        return isStep ? outResult : null;
    }
}
