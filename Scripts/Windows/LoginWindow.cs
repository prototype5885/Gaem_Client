using Godot;
using System;

public partial class LoginWindow : Control
{
    GUI gui;
    LineEdit UsernameInput;
    LineEdit PasswordInput;


    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        UsernameInput = panel.GetChild<LineEdit>(0);
        PasswordInput = panel.GetChild<LineEdit>(1);
        // end

        panel.GetChild<Button>(2).Pressed += () => _on_login_pressed();
        panel.GetChild<Button>(3).Pressed += () => _on_register_pressed();
    }
    void _on_login_pressed()
    {
        int loginCode = gui.clientUDP.Authentication(true, UsernameInput.Text, PasswordInput.Text);
        switch (loginCode)
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
                GD.Print("Connection to the server has been lost.");
                gui.BackToConnectWindow();
                gui.errorLabel.Text = "Connection to the server has been lost.";
                break;
        }
    }
    void _on_register_pressed()
    {
        gui.CloseWindows();
        gui.RegistrationWindow.Visible = true;
    }
}
