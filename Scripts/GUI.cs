using System;
using Godot;

public partial class GUI : Control
{
    [Signal] public delegate void PlayerControlsSignalEventHandler(bool input);

    public Client client;

    public static Control Windows;

    static Control ConnectWindow;
    public Control LoginWindow;
    public Control RegistrationWindow;

    public Label errorLabel;

    public string ip = "";
    public int port = 0;

    public override void _Ready()
    {
        // init
        client = GetNode<Client>("/root/Map/MultiplayerManager");

        Windows = GetChild<Control>(1);

        ConnectWindow = Windows.GetChild<Control>(0);
        LoginWindow = Windows.GetChild<Control>(1);
        RegistrationWindow = Windows.GetChild<Control>(2);
        errorLabel = Windows.GetChild<Label>(3);
        // end

        Windows.Visible = true;

        CloseWindows();

        // ConnectWindow.Visible = true;

        LoginWindow.Visible = true;
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
    public void CloseWindows()
    {
        errorLabel.Text = string.Empty;
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