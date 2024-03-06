using System;
using Godot;

public partial class GUI : Control
{
    [Signal] public delegate void PlayerControlsSignalEventHandler(bool input);

    public override void _Ready()
    {
        PlayerControlsEnabled(false);
    }
    public void PlayerControlsEnabled(bool input)
    {
        if (input == true)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else if (input == false)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        EmitSignal(SignalName.PlayerControlsSignal, input);
    }

}