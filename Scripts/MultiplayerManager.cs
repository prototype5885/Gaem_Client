using Godot;
using System;

public partial class MultiplayerManager : Node
{
    public ENetMultiplayerPeer peer;

    PlayerManager playerManager;

    [Signal] public delegate void PlayerConnectedEventHandler();

    public override void _Ready()
    {
        // init
        playerManager = GetNode<PlayerManager>("/root/Map/PlayerManager");
        // end

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
    }

    private void ConnectionFailed()
    {
        GD.Print("connection failed");
    }

    private void ConnectedToServer()
    {
        //GD.Print("connected");
    }

    void PeerConnected(long id)
    {
        GD.Print("peer connected: " + id);
    }


    public void host_server(int port)
    {
        peer = new ENetMultiplayerPeer();

        var error = peer.CreateServer(port, 32);
        if (error != Error.Ok)
        {
            GD.Print("error hosting: " + error.ToString());
            return;
        }
        //peer.Host.Compress(ENetConnection.CompressionMode.None);

        Multiplayer.MultiplayerPeer = peer;

        playerManager.InstantiatePlayer(peer.GetUniqueId());

        connected();
    }
    public void connect_to_server(string ip, int port)
    {
        peer = new ENetMultiplayerPeer();

        var error = peer.CreateClient(ip, port);
        if (error != Error.Ok)
        {
            GD.Print("error hosting: " + error.ToString());
            return;
        }

        //peer.Host.Compress(ENetConnection.CompressionMode.None);

        Multiplayer.MultiplayerPeer = peer;

        playerManager.InstantiatePlayer(peer.GetUniqueId());

        connected();
    }
    void connected()
    {
        EmitSignal(SignalName.PlayerConnected);
    }
}
