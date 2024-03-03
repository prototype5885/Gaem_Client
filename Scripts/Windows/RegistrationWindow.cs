using Godot;

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

        panel.GetChild<Button>(3).Pressed += () => OnRegisterPressed();
        panel.GetChild<Button>(4).Pressed += () => OnBackPressed();
    }
    void OnRegisterPressed()
    {
        string username = UsernameInput.Text;
        string password = FirstPasswordInput.Text;
        string secondPassword = SecondPasswordInput.Text;

        if (password != secondPassword)
        {
            gui.errorLabel.Text = "Passwords don't match";
            GD.Print("pw doesnt match");
            return;
        }
        if (password.Length < 2)
        {
            gui.errorLabel.Text = "Password is too short, min 2 characters are required";
            return;
        }
        if (username.Length < 2 || username.Length > 16)
        {
            gui.errorLabel.Text = "Username is either too short or too long, min 2 characters, max 16";
            return;
        }

        gui.client.loginOrRegister = false;
        gui.client.Connect(gui.ip, gui.port, username, password);
    }

    public bool RegistrationResult(int receivedCode)
    {
        switch (receivedCode)
        {
            case 1: // Login Successful
                gui.CallDeferred(nameof(gui.CloseWindows));
                gui.CallDeferred(nameof(gui.PlayerControlsEnabled), true);
                return true;
            case 5:
                gui.errorLabel.Text = "Username is either too short or too long, min 2 characters, max 16   ";
                return false;
            case 6:
                gui.errorLabel.Text = "Username is already taken.";
                return false;
            case -1:
                gui.BackToConnectWindow();
                gui.errorLabel.Text = "Failed to connect to the server.";
                return false;
        }
        return false;
    }
    void OnBackPressed()
    {
        gui.CloseWindows();
        gui.LoginWindow.Visible = true;
    }
}
