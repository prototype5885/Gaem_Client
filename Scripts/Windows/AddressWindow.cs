using Godot;

public partial class AddressWindow : Control
{
    private GUI gui;
    private LineEdit InputIP;
    LineEdit InputPort;

    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        InputIP = panel.GetChild<LineEdit>(0);
        InputPort = panel.GetChild<LineEdit>(1);
        // end

        panel.GetChild<Button>(2).Pressed += () => OnSetPressed();

        SetAddress();
    }
    private void SetAddress()
    {
        if (!int.TryParse(InputPort.Text, out int port))
        {
            GD.Print("Wrong port format");
            return;
        }
        string ip = InputIP.Text;

        gui.ip = ip;
        gui.port = port;
    }
    private void OnSetPressed()
    {
        SetAddress();

        gui.CloseWindows();
        gui.LoginWindow.Visible = true;
    }
}
