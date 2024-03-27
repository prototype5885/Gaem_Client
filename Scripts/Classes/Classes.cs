using System.Collections.Generic;
using System.Net;
using System.Numerics;
using Godot;
using Vector3 = Godot.Vector3;

public class Packet
{
    public int type { get; set; }
    public string data { get; set; }
}

public class Player
{
    public bool localPlayer { get; set; }
    public string playerName { get; set; }
    public PlayerPosition serverPosition { get; set; } = new();
    public CharacterBody3D body { get; set; }
    public Node3D head { get; set; }
    public Label3D nameIndicator { get; set; }
    
    public override string ToString()
    {
        string text = $"Self: {localPlayer}, Name: {playerName}, Position: {serverPosition}";

        if (localPlayer) text = $">{text}<";
        return text;
    }

    public void UpdateNameIndicator()
    {
        nameIndicator.Text = playerName;
    }
    public void ConvertLocalPositionToServerFormat()
    {
        serverPosition.x = body.Position.X;
        serverPosition.y = body.Position.Y;
        serverPosition.z = body.Position.Z;

        serverPosition.rx = head.Rotation.X;
        serverPosition.ry = body.Rotation.Y;
    }
}
