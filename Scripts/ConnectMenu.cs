using Godot;
using System;

public partial class ConnectMenu : Control
{
    MultiplayerManager MultiplayerManager;

    Control ConnectWindow;
    Control HostWindow;

    LineEdit InputIP;
    LineEdit InputPort;

    LineEdit InputHostPort;

    Control LoginMenu;

    public override void _Ready()
    {
        // init
        MultiplayerManager = GetNode<MultiplayerManager>("/root/Map/MultiplayerManager");

        ConnectWindow = GetChild<Control>(0);
        HostWindow = GetChild<Control>(1);

        Panel ConnectPanel = ConnectWindow.GetChild<Panel>(0);
        InputIP = ConnectPanel.GetChild<LineEdit>(0);
        InputPort = ConnectPanel.GetChild<LineEdit>(1);

        Panel HostPanel = HostWindow.GetChild<Panel>(0);
        InputHostPort = HostPanel.GetChild<LineEdit>(0);

        LoginMenu = GetParent().GetChild<Control>(1);
        // end

        Visible = true;
        ConnectWindow.Visible = true;
        HostWindow.Visible = false;

    }
    void _on_to_host_pressed()
    {
        ConnectWindow.Visible = false;
        HostWindow.Visible = true;
    }
    void _on_back_pressed()
    {
        ConnectWindow.Visible = true;
        HostWindow.Visible = false;
    }
    void _on_connect_pressed()
    {
        int port;

        if (!int.TryParse(InputPort.Text, out port)) 
        { 
            return; 
        }

        string ip = InputIP.Text;

        MultiplayerManager.connect_to_server(ip, port);

        Visible = false;
        //LoginMenu.Visible = true;
    }
    void _on_host_pressed()
    {
        int port;

        if (!int.TryParse(InputHostPort.Text, out port))
        {
            return;
        }

        MultiplayerManager.host_server(port);

        Visible = false;
        //LoginMenu.Visible = true;
    }
}
