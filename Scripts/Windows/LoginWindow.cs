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
    }
    void _on_login_pressed()
    {
        bool loginSuccessful = gui.mpClient.Authentication(UsernameInput.Text, PasswordInput.Text);

        if (loginSuccessful)
        {
            Visible = false;
            gui.PlayerControlsEnabled(true);
        }
    }
}
