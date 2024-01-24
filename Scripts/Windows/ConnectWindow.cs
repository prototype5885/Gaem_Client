using Godot;
using System;

public partial class ConnectWindow : Control
{
    GUI gui;
    LineEdit InputIP;
    LineEdit InputPort;

    public override void _Ready()
    {
        // init
        gui = GetParent<Control>().GetParent<GUI>();
        Panel panel = GetChild<Panel>(0);
        InputIP = panel.GetChild<LineEdit>(0);
        InputPort = panel.GetChild<LineEdit>(1);
        // end

        panel.GetChild<Button>(2).Pressed += () => _on_connect_pressed();
    }

    void _on_connect_pressed()
    {
        if (!int.TryParse(InputPort.Text, out int port))
        {
            return;
        }

        string ip = InputIP.Text;
        gui.mpClient.Connect(ip, port); // idk why it takes long to open
        if (!gui.mpClient.isConnected) { return; }
        gui.CloseWindows();
        gui.LoginWindow.Visible = true;
    }
}