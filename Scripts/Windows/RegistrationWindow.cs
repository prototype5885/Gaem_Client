using Godot;
using System;

public partial class RegistrationWindow : Control
{
    GUI gui;

    LineEdit UsernameInput;
    LineEdit FirstPasswordInput;
    LineEdit SecondPasswordInput;

    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        UsernameInput = panel.GetChild<LineEdit>(0);
        FirstPasswordInput = panel.GetChild<LineEdit>(1);
        SecondPasswordInput = panel.GetChild<LineEdit>(2);
        // end

        panel.GetChild<Button>(3).Pressed += () => _on_register_pressed();
        panel.GetChild<Button>(4).Pressed += () => _on_back_pressed();
    }
    void _on_register_pressed()
    {
        string username = UsernameInput.Text;
        string password = FirstPasswordInput.Text;
        string secondPassword = SecondPasswordInput.Text;

        if (password != secondPassword) { return; }

        bool registerSuccessful = gui.mpClient.Authentication(false, username, password);

        if (registerSuccessful)
        {
            gui.CloseWindows();
            gui.PlayerControlsEnabled(true);
        }
    }
    void _on_back_pressed()
    {
        gui.CloseWindows();
        gui.LoginWindow.Visible = true;
    }
}
