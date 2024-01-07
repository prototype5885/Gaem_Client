using Godot;
using System;

public partial class Host : Node
{
    MultiplayerManager MultiplayerManager;
    //PlayerManager playerManager;

    public override void _Ready()
    {
        // init
        //playerManager = GetNode<PlayerManager>("/root/Map/PlayerManager");
        MultiplayerManager = GetParent<MultiplayerManager>();
        // end

        //Multiplayer.PeerConnected += PeerConnected;
        //Multiplayer.PeerDisconnected += PeerDisconnected;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("host"))
            {
                HostServer(1942);
                //SetProcessInput(false);
            }
        }
    }
    public void HostServer(int port)
    {
        if (MultiplayerManager.peer == null)
        {
            MultiplayerManager.peer = new ENetMultiplayerPeer();
            var error = MultiplayerManager.peer.CreateServer(port, 32);
            if (error != Error.Ok)
            {
                GD.Print("error hosting: " + error.ToString());
                return;
            }
            MultiplayerManager.peer.Host.Compress(ENetConnection.CompressionMode.None);

            Multiplayer.MultiplayerPeer = MultiplayerManager.peer;
            GD.Print("Hosted");
        }
        else
        {
            GD.Print("Already in server");
        }

        //add_player(1);
    }
    //void add_player(long id)
    //{
    //    playerManager.InstantiatePlayer((int)id);
    //}
    //void PeerConnected(long id)
    //{
    //    GD.Print("peer connected: " + id);
    //    //add_player(id);
    //}
    //void PeerDisconnected(long id)
    //{
    //    GD.Print("disconnected: " + id);
    //}
}