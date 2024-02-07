using Godot;

public partial class RegistrationWindow : Control
{
    private GUI gui;

    private LineEdit UsernameInput;
    private LineEdit FirstPasswordInput;
    private LineEdit SecondPasswordInput;

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
    private void _on_register_pressed()
    {
        string username = UsernameInput.Text;
        string password = FirstPasswordInput.Text;
        string secondPassword = SecondPasswordInput.Text;

        if (password != secondPassword)
        {
            gui.errorLabel.Text = "Passwords don't match";
            return;
        }

        gui.clientUdp.loginOrRegister = false;
        gui.clientUdp.Connect(username, password);
    }

    public void RegistrationResult(int receivedCode)
    {
        switch (receivedCode)
        {
            case 1: // Login Successful
                gui.CloseWindows();
                gui.PlayerControlsEnabled(true);
                break;
            case 2:
                gui.errorLabel.Text = "Max 16 characters allowed for username";
                break;
            case 3:
                gui.errorLabel.Text = "Username is already taken.";
                break;
            case -1:
                gui.BackToConnectWindow();
                gui.errorLabel.Text = "Connection to the server has been lost.";
                break;
        }
    }
    private void _on_back_pressed()
    {
        gui.CloseWindows();
        gui.LoginWindow.Visible = true;
    }
}
