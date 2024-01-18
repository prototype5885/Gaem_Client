// ClientNode.cs
using Godot;
using System.Net.Sockets;

public partial class ClientUDP : Node
{
    private PacketPeerUdp udp = new PacketPeerUdp();

    private bool connected = false;

    public override void _Ready()
    {
        SetProcess(false);
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("join"))
            {
                udp.ConnectToHost("127.0.0.1", 1942);
                SetProcess(true);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!connected)
        {
            // Try to contact server
            udp.PutPacket("The Answer Is..42!".ToUtf8Buffer());
        }
        if (udp.GetAvailablePacketCount() > 0)
        {
            GD.Print($"Connected: {udp.GetPacket().GetStringFromUtf8()}");
            connected = true;
        }
    }
}