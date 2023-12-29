using Godot;
using System;

public partial class Head : Camera3D
{
    CharacterBody3D playerController;

    // mouselook
    [Export] float mouse_sens = 1.5f;
    public float mouse_sens_multiplier = 1f;

    float player_rotation_horizontal = 0f;
    float player_rotation_vertical = 0f;

    float vertical_clamp_angle;

    float playerFOV;

    public override void _Ready()
    {
        // init
        playerController = GetOwner<CharacterBody3D>();
        // end

        vertical_clamp_angle = Mathf.DegToRad(85f);

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    public override void _Input(InputEvent @event)
    {
        // get mouse input for camera rotation
        if (@event is InputEventMouseMotion)
        {
            // calculates horizontal aim
            InputEventMouseMotion e = (InputEventMouseMotion)@event;
            playerController.RotateY(Mathf.DegToRad(-e.Relative.X * mouse_sens * mouse_sens_multiplier / 20));
            // calculates vertical aim
            RotateX(Mathf.DegToRad(-e.Relative.Y * mouse_sens / 20));
            Rotation = Rotation with { X = Mathf.Clamp(Rotation.X, -vertical_clamp_angle, vertical_clamp_angle) };
        }
    }
}
