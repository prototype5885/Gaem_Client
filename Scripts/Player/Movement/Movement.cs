using Godot;
using System;
using static Godot.TextServer;

public partial class Movement : CharacterBody3D
{
    Camera3D camera;

    // speed multipliers
    const float default_speed = 4.0f;
    float sprint_speed_multiplier = 1f;

    float total_speed_multiplier;

    // acceleration
    const float accel_default = 10f;
    const float accel_air = 1f;
    float accel = accel_default;

    // input
    Vector3 direction;
    float h_rot;
    float f_input;
    float h_input;

    // tilt
    float target_tilt;
    float current_tilt = 0f;

    // normalized speed
    float movementspeed;

    // gravity
    float gravity = 9.8f;
    float gravity_multiplier = 1f;

    bool falling = false;

    bool canMove = true;

    public override void _Ready()
    {
        // init
        camera = GetNode<Camera3D>("Head");
        // end

        GetNode<GUI>("/root/Map/GUI").PlayerControlsSignal += _player_controls_enabled;

        change_speed();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (IsOnFloor())
            {
                if (Input.IsActionJustPressed("jump"))
                {
                    Velocity = Velocity with { Y = 5 };
                }

                if (Input.IsActionJustPressed("sprint"))
                {
                    sprint_speed_multiplier = 1.5f;
                    change_speed();
                }
                if (Input.IsActionJustReleased("sprint"))
                {
                    sprint_speed_multiplier = 1.0f;
                    change_speed();
                }
            }
        }
    }
    void _player_controls_enabled(bool input)
    {
        SetProcessInput(input);
        canMove = input;
    }
    public override void _PhysicsProcess(double delta)
    {
        movement((float)delta);
        MoveAndSlide();
    }
    void movement(float delta)
    {
        // declares local velocity to be able to modify it
        Vector3 localVelocity = Velocity;

        // gets input from WASD
        direction = Vector3.Zero;
        h_rot = GlobalTransform.Basis.GetEuler().Y;

        if (canMove)
        {
            f_input = Input.GetActionStrength("move_backwards") - Input.GetActionStrength("move_forwards");
            h_input = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        }
        else
        {
            f_input = 0;
            h_input = 0;
        }

        direction = new Vector3(h_input, 0, f_input).Rotated(Vector3.Up, h_rot).Normalized();

        // normal acceleration on floor
        if (IsOnFloor())
        {
            accel = accel_default;
            falling = false;
        }

        // applies gravity and lowers acceleration in the air
        if (!IsOnFloor())
        {
            localVelocity.Y -= gravity * delta;
            accel = accel_air;
            falling = true;
        }

        // sets velocity
        localVelocity = localVelocity.Lerp(direction * total_speed_multiplier, accel * delta);

        // limits fall speed
        localVelocity.Y = Mathf.Clamp(localVelocity.Y, -50f, 1000f);

        // player speed var
        movementspeed = localVelocity.Length();

        // sets calculated local velocity as real velocity
        Velocity = localVelocity;
    }
    void change_speed()
    {
        total_speed_multiplier = default_speed * sprint_speed_multiplier;
    }
}
