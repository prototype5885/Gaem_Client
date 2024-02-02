using Godot;

public partial class GUI : Control
{
    [Signal] public delegate void PlayerControlsSignalEventHandler(bool input);

    public ClientUDP clientUDP;

    Control Windows;

    public Control ConnectWindow;
    public Control LoginWindow;
    public Control RegistrationWindow;

    public Label errorLabel;

    public override void _Ready()
    {
        // init
        clientUDP = GetNode<ClientUDP>("/root/Map/MultiplayerManager/ClientUDP");

        Windows = GetChild<Control>(1);

        ConnectWindow = Windows.GetChild<Control>(0);
        LoginWindow = Windows.GetChild<Control>(1);
        RegistrationWindow = Windows.GetChild<Control>(2);
        errorLabel = Windows.GetChild<Label>(3);
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
            if (window != errorLabel)
            {
                window.Visible = false;
            }
        }
    }
    public void BackToConnectWindow()
    {
        CloseWindows();
        ConnectWindow.Visible = true;
    }
}