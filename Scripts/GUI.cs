using Godot;

public partial class GUI : Control
{
    [Signal] public delegate void PlayerControlsSignalEventHandler(bool input);

    public TCPClient tcpClient;

    Control Windows;

    public Control ConnectWindow;
    public Control LoginWindow;
    public Control RegistrationWindow;


    public override void _Ready()
    {
        // init
        tcpClient = GetNode<TCPClient>("/root/Map/MultiplayerManager/TCPClient");

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