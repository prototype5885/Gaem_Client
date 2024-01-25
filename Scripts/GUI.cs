using Godot;

public partial class GUI : Control
{
    [Signal] public delegate void PlayerControlsSignalEventHandler(bool input);

    public MultiplayerClient mpClient;

    Control Windows;

    public Control ConnectWindow;
    public Control LoginWindow;
    public Control RegistrationWindow;


    public override void _Ready()
    {
        // init
        mpClient = GetNode<MultiplayerClient>("/root/Map/MultiplayerManager");

        Windows = GetChild<Control>(1);

        ConnectWindow = Windows.GetChild<Control>(0);
        LoginWindow = Windows.GetChild<Control>(1);
        RegistrationWindow = Windows.GetChild<Control>(2);
        // end

        Windows.Visible = true;

        CloseWindows();

        ConnectWindow.Visible = true;

        PlayerControlsEnabled(false);
    }
    public void PlayerControlsEnabled(bool input)
    {
        if (input)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        EmitSignal(SignalName.PlayerControlsSignal, input);
    }
    public void CloseWindows()
    {
        foreach (Control window in Windows.GetChildren())
        {
            window.Visible = false;
        }
    }
}