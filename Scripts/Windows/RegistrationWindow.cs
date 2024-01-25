using Godot;
using System;

public partial class RegistrationWindow : Control
{
    GUI gui;

    LineEdit UsernameInput;
    LineEdit FirstPasswordInput;
    LineEdit SecondPasswordInput;
    Label ErrorLabel;

    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        UsernameInput = panel.GetChild<LineEdit>(0);
        FirstPasswordInput = panel.GetChild<LineEdit>(1);
        SecondPasswordInput = panel.GetChild<LineEdit>(2);
        ErrorLabel = panel.GetChild<Label>(5);
        // end

        panel.GetChild<Button>(3).Pressed += () => _on_register_pressed();
        panel.GetChild<Button>(4).Pressed += () => _on_back_pressed();
    }
    void _on_register_pressed()
    {
        string username = UsernameInput.Text;
        string password = FirstPasswordInput.Text;
        string secondPassword = SecondPasswordInput.Text;

        if (password != secondPassword)
        {
            ErrorLabel.Text = "Passwords don't match";
            return;
        }

        int registerCode = gui.mpClient.Authentication(false, username, password);

        if (registerCode == 1)
        {
            gui.CloseWindows();
            gui.PlayerControlsEnabled(true);
        }
        else if (registerCode == 2)
        {
            ErrorLabel.Text = "Max 16 characters allowed for username";
        }
        else if (registerCode == 3)
        {
            ErrorLabel.Text = "Username is already taken.";
        }
    }
    void _on_back_pressed()
    {
        gui.CloseWindows();
        gui.LoginWindow.Visible = true;
    }
}
