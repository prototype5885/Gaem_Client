using Godot;

public partial class GUI : Control
{
    [Signal] public delegate void InputEnabledEventHandler(bool input);
    // Movement playerMovement;
    // Head playerHead;

    // public override void _Ready()
    // {
    //     Node playerManager = GetNode<Node>("/root/Map/PlayerManager");

    //     playerMovement = playerManager.GetChild<Movement>(0);
    //     playerHead = playerMovement.GetChild<Head>(0);
    //     GD.Print(playerMovement);
    //     GD.Print(playerHead);
    // }

    public void input_enabled(bool input)
    {
        EmitSignal(SignalName.InputEnabled, input);
    }
}