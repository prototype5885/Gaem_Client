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
        try
        {
            //gui.mpClient.StatusLabel.Text = "Connecting...";

            if (gui.ip == "localhost") { gui.ip = "127.0.0.1"; }

            gui.clientUdp.serverAddress = gui.ip;
            gui.clientUdp.serverPort = gui.port;

            //if (gui.clientUDP.Connect(ip, port)) // Note: dont use "localhost" for ip cuz slow
            //{
            gui.CloseWindows();
            gui.LoginWindow.Visible = true;
            //}
        }
        catch (Exception ex)
        {
            gui.errorLabel.Text = "Connection failed";
            GD.Print(ex);
        }

        string username = UsernameInput.Text;
        string password = PasswordInput.Text;
        //int loginCode = gui.clientUDP.Authentication(true, UsernameInput.Text, PasswordInput.Text); // Pressed the login button
        gui.clientUdp.loginOrRegister = true;
        gui.clientUdp.Connect(username, password);

    }

    public void LoginResult(int receivedCode)
    {
        switch (receivedCode)
        {
            case 1: // Login Successful
                GD.Print("Login was successful");
                gui.CloseWindows();
                gui.PlayerControlsEnabled(true);
                break;
            case 2:
                GD.Print("Wrong username or password.");
                gui.errorLabel.Text = "Wrong username or password.";
                break;
            case -1:
                GD.Print("Failed to connect to the server.");
                // gui.BackToConnectWindow();
                gui.errorLabel.Text = "Failed to connect to the server.";
                break;
        }
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
