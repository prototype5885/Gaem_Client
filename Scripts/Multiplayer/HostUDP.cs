// ServerNode.cs
using Godot;
using System.Collections.Generic;

public partial class HostUDP : Node
{
    private UdpServer _server = new UdpServer();
    private List<PacketPeerUdp> _peers = new List<PacketPeerUdp>();

    public override void _Ready()
    {
        SetProcess(false);
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("host"))
            {
                _server.Listen(1942);
                SetProcess(true);
            }
        }
    }

    public override void _Process(double delta)
    {
        _server.Poll(); // Important!
        if (_server.IsConnectionAvailable())
        {
            PacketPeerUdp peer = _server.TakeConnection();
            byte[] packet = peer.GetPacket();
            GD.Print($"Accepted Peer: {peer.GetPacketIP()}:{peer.GetPacketPort()}");
            GD.Print($"Received Data: {packet.GetStringFromUtf8()}");
            GetNode<Label>("Label").Text = packet.GetStringFromUtf8();
            // Reply so it knows we received the message.
            peer.PutPacket(packet);
            // Keep a reference so we can keep contacting the remote peer.
            _peers.Add(peer);
        }
        foreach (var peer in _peers)
        {
            // Do something with the peers.
        }
    }
}