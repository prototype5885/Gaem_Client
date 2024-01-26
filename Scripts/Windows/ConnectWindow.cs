using Godot;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

public partial class ConnectWindow : Control
{
    GUI gui;
    LineEdit InputIP;
    LineEdit InputPort;

    string ip = "";

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("join"))
            {
                _on_connect_pressed();
            }
        }
    }
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
        try
        {
            //gui.mpClient.StatusLabel.Text = "Connecting...";
            if (!int.TryParse(InputPort.Text, out int port))
            {
                GD.Print("Wrong port format");
                return;
            }

            string ip = InputIP.Text;

            if (ip == "localhost") { ip = "127.0.0.1"; }

            gui.mpClient.Connect(ip, port); // Note: dont use "localhost" for ip
            if (!gui.mpClient.isConnected)
            {
                return;
            }
            gui.CloseWindows();
            gui.LoginWindow.Visible = true;
        }
        catch (Exception ex)
        {
            GD.Print(ex);
        }
    }
}
