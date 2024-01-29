using Godot;
using System;

public partial class LoginWindow : Control
{
    GUI gui;
    LineEdit UsernameInput;
    LineEdit PasswordInput;
    Label ErrorLabel;


    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        UsernameInput = panel.GetChild<LineEdit>(0);
        PasswordInput = panel.GetChild<LineEdit>(1);
        ErrorLabel = panel.GetChild<Label>(4);
        // end

        panel.GetChild<Button>(2).Pressed += () => _on_login_pressed();
        panel.GetChild<Button>(3).Pressed += () => _on_register_pressed();
    }
    void _on_login_pressed()
    {
        int loginCode = gui.tcpClient.Authentication(true, UsernameInput.Text, PasswordInput.Text);

        if (loginCode == 1)
        {
            gui.CloseWindows();
            gui.PlayerControlsEnabled(true);
        }
        else if (loginCode == 0)
        {
            ErrorLabel.Text = "Wrong username or password.";
        }
    }
    void _on_register_pressed()
    {
        gui.CloseWindows();
        gui.RegistrationWindow.Visible = true;
    }
}
