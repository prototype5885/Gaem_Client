using Godot;
using System;
using System.Collections.Generic;

public partial class PlayersManager : Node3D
{
    PackedScene playerScene = GD.Load<PackedScene>("res://Components/Player.tscn");
    MultiplayerClient mpClient;
    CharacterBody3D player;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        mpClient = GetNode<MultiplayerClient>("/root/Map/MultiplayerManager");
    }
    public void SpawnPlayer()
    {
        player = playerScene.Instantiate() as CharacterBody3D;
        GetChild(0).AddChild(player);
        player.Position = new Vector3(0f, 3f, 0f);
        SetPhysicsProcess(true);
    }
    public override void _PhysicsProcess(double delta)
    {
        mpClient.localPlayerPosition.x = player.Position.X;
        mpClient.localPlayerPosition.y = player.Position.Y;
        mpClient.localPlayerPosition.z = player.Position.Z;
    }
    public void ProcessOtherPlayerPosition(Dictionary<int, (float, float, float)> everyPlayerPosition)
    {
        foreach (var kvp in everyPlayerPosition)
        {
            Console.WriteLine(kvp);
        }
    }
}
