using Godot;

public partial class Client : Node
{
    MultiplayerManager MultiplayerManager;
    public override void _Ready()
    {
        // init
        MultiplayerManager = GetParent<MultiplayerManager>();
        // end
        Multiplayer.ConnectedToServer += SuccessfullyConnected;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("join"))
            {
                ConnectToServer("127.0.0.1", 1942);
                //SetProcessInput(false);
            }
        }
    }
    public void ConnectToServer(string ip, int port)
    {
        if (MultiplayerManager.peer == null)
        {
            MultiplayerManager.peer = new ENetMultiplayerPeer();

            var error = MultiplayerManager.peer.CreateClient(ip, port);
            if (error != Error.Ok)
            {
                GD.Print("error hosting: " + error.ToString());
                return;
            }

            MultiplayerManager.peer.Host.Compress(ENetConnection.CompressionMode.None);

            Multiplayer.MultiplayerPeer = MultiplayerManager.peer;
            GD.Print("Connected to server");
        }
        else
        {
            GD.Print("Already in server");
        }
        
    }
    void SuccessfullyConnected() // runs when connection was successfull
    {
        RpcId(1, "connected"); // tell the server that the client connected
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    void connected()
    {
        GD.Print("Peer connected: " + Multiplayer.GetUniqueId());
    }
}