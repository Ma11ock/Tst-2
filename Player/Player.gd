extends CharacterBody3D

@onready var body = $Body
@onready var head = $Body/Head
@onready var camera = $Body/Head/CameraMarker3D/Camera3D
@onready var camera_target = $Body/Head/CameraMarker3D
@onready var head_position: Vector3 = head.position

const ACCELERATION_DEFAULT: float = 7.0
const ACCELERATION_AIR: float = 1.0
const SPEED_DEFAULT: float = 7.0
const SPEED_ON_STAIRS: float = 5.0

var speed: float = SPEED_DEFAULT

var gravity: float = 15.34
var main_velocity: Vector3 = Vector3.ZERO
var gravity_direction: Vector3 = Vector3.ZERO
var movement: Vector3 = Vector3.ZERO

const STAIRS_FEELING_COEFFICIENT: float = 2.5
const WALL_MARGIN: float = 0.001
const STEP_DOWN_MARGIN: float = 0.01
const STEP_HEIGHT_DEFAULT: Vector3 = Vector3(0, 0.6, 0)
const STEP_MAX_SLOPE_DEGREE: float = 40.0
const STEP_CHECK_COUNT: int = 2
const SPEED_CLAMP_AFTER_JUMP_COEFFICIENT = 0.4
const SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT = 0.4

var step_check_height: Vector3 = STEP_HEIGHT_DEFAULT / STEP_CHECK_COUNT
var is_jumping: bool = false
var is_in_air: bool = false

var head_offset: Vector3 = Vector3.ZERO
var camera_target_position : Vector3 = Vector3.ZERO
var camera_lerp_coefficient: float = 1.0
var time_in_air: float = 0.0
var update_camera = false
var camera_gt_previous : Transform3D
var camera_gt_current : Transform3D

# Movement
const MAX_VELOCITY_AIR = 0.6
const MAX_VELOCITY_GROUND = 8.0
const MAX_ACCELERATION = 10 * MAX_VELOCITY_GROUND
const STOP_SPEED = 2.0
const JUMP_IMPULSE = sqrt(2.0 * 9.8 * 0.85) * 1.4
const SMOOTHNESS = 10.0

var friction: = 4.0

var mouse_sensitivity := 0.2
var direction := Vector3.ZERO
var wish_jump := false

# Smooth camera
var camera_input := Vector2.ZERO
var camera_rotation_velocity := Vector2.ZERO

class StepResult:
    var diff_position: Vector3 = Vector3.ZERO
    var normal: Vector3 = Vector3.ZERO
    var is_step_up: bool = false


func _ready():
    Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
    
    camera_target_position = camera.global_transform.origin
    camera.set_as_top_level(true)
    camera.global_transform = camera_target.global_transform
    
    camera_gt_previous = camera_target.global_transform
    camera_gt_current = camera_target.global_transform

func update_camera_transform():
    camera_gt_previous = camera_gt_current
    camera_gt_current = camera_target.global_transform
    
func _process(delta: float) -> void:
    #pass
    if update_camera:
        update_camera_transform()
        update_camera = false

    var interpolation_fraction = clamp(Engine.get_physics_interpolation_fraction(), 0, 1)

    var camera_xform = camera_gt_previous.interpolate_with(camera_gt_current, interpolation_fraction)
    camera.global_transform = camera_xform

    var head_xform : Transform3D = head.get_global_transform()
    
    camera_target_position = lerp(camera_target_position, head_xform.origin, delta * speed * STAIRS_FEELING_COEFFICIENT * camera_lerp_coefficient)

    if is_on_floor():
        time_in_air = 0.0
        camera_lerp_coefficient = 1.0
        camera.position.y = camera_target_position.y
    else:
        time_in_air += delta
        if time_in_air > 1.0:
            camera_lerp_coefficient += delta
            camera_lerp_coefficient = clamp(camera_lerp_coefficient, 2.0, 4.0)
        else: 
            camera_lerp_coefficient = 2.0

        camera.position.y = camera_target_position.y

func _input(event):
    if event is InputEventMouseMotion:
        if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
            camera_input = event.relative
            body.rotate_y(deg_to_rad(-event.relative.x * mouse_sensitivity))
            head.rotate_x(deg_to_rad(-event.relative.y * mouse_sensitivity))
            head.rotation.x = clamp(head.rotation.x, deg_to_rad(-89), deg_to_rad(89))
    elif event is InputEventMouseButton:
        if event.is_action_pressed("ui_click") and Input.get_mouse_mode() != Input.MOUSE_MODE_CAPTURED:
            Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
    elif event is InputEventKey:
        if event.is_action_pressed("ui_activate_mouse") and Input.get_mouse_mode() != Input.MOUSE_MODE_VISIBLE:
            Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
        elif event.is_action_pressed("ui_fullscreen"):
            if DisplayServer.window_get_mode() != DisplayServer.WINDOW_MODE_FULLSCREEN:
                DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
            else:
                DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)

func _physics_process(delta):
    update_camera = true
    var is_step: bool = false
    
    var input = Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
    direction = (body.global_transform.basis * Vector3(input.x, 0, input.y)).normalized()
    
    wish_jump = Input.is_action_just_pressed("move_jump")

    var accel := 1.0
    if is_on_floor():
        is_jumping = false
        is_in_air = false
        accel = 7.0
        if wish_jump:
            is_jumping = true
            wish_jump = false
            main_velocity = _update_velocity_air(direction, delta)
            gravity_direction = Vector3.UP * JUMP_IMPULSE
        else:
            main_velocity = _update_velocity_ground(direction, delta)
            gravity_direction = Vector3.ZERO
    else:
        is_in_air = true
        #velocity.y -= gravity * delta
        main_velocity = _update_velocity_air(direction, delta)
        gravity_direction += Vector3.DOWN * gravity * delta

    var step_result : StepResult = StepResult.new()
    
    is_step = step_check(delta, is_jumping, step_result)
        
    if is_step:
        if step_result.is_step_up:
            if is_in_air:
                main_velocity *= SPEED_CLAMP_AFTER_JUMP_COEFFICIENT
                gravity_direction *= SPEED_CLAMP_AFTER_JUMP_COEFFICIENT
            else:
                if direction.dot(step_result.normal) > 0:
                    global_transform.origin += main_velocity * delta
                    main_velocity *= SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT
                    gravity_direction *= SPEED_CLAMP_SLOPE_STEP_UP_COEFFICIENT

        global_transform.origin += step_result.diff_position
        head_offset = step_result.diff_position
        speed = SPEED_ON_STAIRS
    else:
        head_offset = head_offset.lerp(Vector3.ZERO, delta * speed * STAIRS_FEELING_COEFFICIENT)
        
        if abs(head_offset.y) <= 0.01:
            speed = SPEED_DEFAULT

    main_velocity.y = 0
    movement = main_velocity + gravity_direction

    set_velocity(movement)
    set_max_slides(6)
    move_and_slide()
    
    if is_jumping:
        is_jumping = false
        is_in_air = true
        
func _accelerate(wish_dir: Vector3, max_velocity: float, delta: float) -> Vector3:
    var vy := velocity.y
    velocity.y = 0
    var current_speed = velocity.dot(wish_dir)
    
    var add_speed = clamp(max_velocity - current_speed, 0, MAX_ACCELERATION * delta)
    
    return (velocity + add_speed * wish_dir) + Vector3(0, vy, 0)

func _update_velocity_ground(wish_dir: Vector3, delta: float) -> Vector3:
    var speed = velocity.length()
    
    if speed != 0:
        var control = max(STOP_SPEED, speed)
        var drop = control * friction * delta
        
        # Scale the velocity based off of friction
        velocity *= max(speed - drop, 0) / speed
    
    return _accelerate(wish_dir, MAX_VELOCITY_GROUND, delta)

func _update_velocity_air(wish_dir: Vector3, delta: float) -> Vector3:
    return _accelerate(wish_dir, MAX_VELOCITY_AIR, delta)

func step_check(delta: float, is_jumping_: bool, step_result: StepResult):
    var is_step: bool = false
    
    if gravity_direction.y >= 0:
        for i in range(STEP_CHECK_COUNT):
            var test_motion_result: PhysicsTestMotionResult3D = PhysicsTestMotionResult3D.new()
            
            var step_height: Vector3 = STEP_HEIGHT_DEFAULT - i * step_check_height
            var transform3d: Transform3D = global_transform
            var motion: Vector3 = step_height
            var test_motion_params: PhysicsTestMotionParameters3D = PhysicsTestMotionParameters3D.new()
            test_motion_params.from = transform3d
            test_motion_params.motion = motion
            
            var is_player_collided: bool = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)

            if is_player_collided and test_motion_result.get_collision_normal().y < 0:
                continue

            transform3d.origin += step_height
            motion = main_velocity * delta
            test_motion_params.from = transform3d
            test_motion_params.motion = motion
            
            is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
            
            if not is_player_collided:
                transform3d.origin += motion
                motion = -step_height
                test_motion_params.from = transform3d
                test_motion_params.motion = motion
                
                is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
                
                if is_player_collided:
                    if test_motion_result.get_collision_normal().angle_to(Vector3.UP) <= deg_to_rad(STEP_MAX_SLOPE_DEGREE):
                        is_step = true
                        step_result.is_step_up = true
                        step_result.diff_position = -test_motion_result.get_remainder()
                        step_result.normal = test_motion_result.get_collision_normal()
                        break
            else:
                var wall_collision_normal: Vector3 = test_motion_result.get_collision_normal()
                transform3d.origin += wall_collision_normal * WALL_MARGIN
                motion = (main_velocity * delta).slide(wall_collision_normal)
                test_motion_params.from = transform3d
                test_motion_params.motion = motion
                
                is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
                
                if not is_player_collided:
                    transform3d.origin += motion
                    motion = -step_height
                    test_motion_params.from = transform3d
                    test_motion_params.motion = motion
                    
                    is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
                    
                    if is_player_collided:
                        if test_motion_result.get_collision_normal().angle_to(Vector3.UP) <= deg_to_rad(STEP_MAX_SLOPE_DEGREE):
                            is_step = true
                            step_result.is_step_up = true
                            step_result.diff_position = -test_motion_result.get_remainder()
                            step_result.normal = test_motion_result.get_collision_normal()
                            break

    if not is_jumping_ and not is_step and is_on_floor():
        step_result.is_step_up = false
        var test_motion_result: PhysicsTestMotionResult3D = PhysicsTestMotionResult3D.new()
        var transform3d: Transform3D = global_transform
        var motion: Vector3 = main_velocity * delta
        var test_motion_params: PhysicsTestMotionParameters3D = PhysicsTestMotionParameters3D.new()
        test_motion_params.from = transform3d
        test_motion_params.motion = motion
        test_motion_params.recovery_as_collision = true

        var is_player_collided: bool = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
            
        if not is_player_collided:
            transform3d.origin += motion
            motion = -STEP_HEIGHT_DEFAULT
            test_motion_params.from = transform3d
            test_motion_params.motion = motion
            
            is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
            
            if is_player_collided and test_motion_result.get_travel().y < -STEP_DOWN_MARGIN:
                if test_motion_result.get_collision_normal().angle_to(Vector3.UP) <= deg_to_rad(STEP_MAX_SLOPE_DEGREE):
                    is_step = true
                    step_result.diff_position = test_motion_result.get_travel()
                    step_result.normal = test_motion_result.get_collision_normal()
        elif is_zero_approx(test_motion_result.get_collision_normal().y):
            var wall_collision_normal: Vector3 = test_motion_result.get_collision_normal()
            transform3d.origin += wall_collision_normal * WALL_MARGIN
            motion = (main_velocity * delta).slide(wall_collision_normal)
            test_motion_params.from = transform3d
            test_motion_params.motion = motion
            
            is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
            
            if not is_player_collided:
                transform3d.origin += motion
                motion = -STEP_HEIGHT_DEFAULT
                test_motion_params.from = transform3d
                test_motion_params.motion = motion
                
                is_player_collided = PhysicsServer3D.body_test_motion(self.get_rid(), test_motion_params, test_motion_result)
                
                if is_player_collided and test_motion_result.get_travel().y < -STEP_DOWN_MARGIN:
                    if test_motion_result.get_collision_normal().angle_to(Vector3.UP) <= deg_to_rad(STEP_MAX_SLOPE_DEGREE):
                        is_step = true
                        step_result.diff_position = test_motion_result.get_travel()
                        step_result.normal = test_motion_result.get_collision_normal()

    return is_step
