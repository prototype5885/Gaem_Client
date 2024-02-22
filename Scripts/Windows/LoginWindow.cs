using Godot;
using System;

public partial class LoginWindow : Control
{
    private GUI gui;
    private LineEdit UsernameInput;
    private LineEdit PasswordInput;

    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        UsernameInput = panel.GetChild<LineEdit>(0);
        PasswordInput = panel.GetChild<LineEdit>(1);
        // end

        panel.GetChild<Button>(2).Pressed += () => OnLoginPressed();
        panel.GetChild<Button>(3).Pressed += () => OnRegisterPressed();
        panel.GetChild<Button>(4).Pressed += () => OnChangeServerPressed();
    }
    private void OnLoginPressed()
    {
        //try
        //{
        //    //gui.mpClient.StatusLabel.Text = "Connecting...";

        //    if (gui.ip == "localhost") { gui.ip = "127.0.0.1"; }


        //    //if (gui.clientUDP.Connect(ip, port)) // Note: dont use "localhost" for ip cuz slow
        //    //{
        //    gui.CloseWindows();
        //    gui.LoginWindow.Visible = true;
        //    //}
        //}
        //catch (Exception ex)
        //{
        //    gui.errorLabel.Text = "Connection failed";
        //    GD.Print(ex);
        //}

        string username = UsernameInput.Text;
        string password = PasswordInput.Text;

        gui.client.loginOrRegister = true;
        gui.client.Connect(gui.ip, gui.port, username, password);
    }
    public bool LoginResult(int receivedCode)
    {
        switch (receivedCode)
        {
            case 1: // Login Successful
                gui.CloseWindows();
                gui.CallDeferred(nameof(gui.CloseWindows));
                gui.CallDeferred(nameof(gui.PlayerControlsEnabled), true);
                return true;
            case 2:
                gui.errorLabel.Text = "Wrong username or password.";
                return false;
            case 3:
                gui.errorLabel.Text = "No user was found with this name";
                return false;
            case 4:
                gui.errorLabel.Text = "This user is already logged in";
                return false;
            case -1:
                // gui.BackToConnectWindow();
                gui.errorLabel.Text = "Failed to connect to the server.";
                return false;
        }
        return false;
    }
    private void OnRegisterPressed()
    {
        gui.CloseWindows();
        gui.RegistrationWindow.Visible = true;
    }

    private void OnChangeServerPressed()
    {
        gui.CloseWindows();
        gui.BackToConnectWindow();
    }
}
