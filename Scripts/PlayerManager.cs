using Godot;
using System;

public partial class PlayerManager : Node
{
    PackedScene playerScene;

    public override void _Ready()
    {
        // init
        playerScene = GD.Load<PackedScene>("res://Components/Player.tscn");
        // end
    }
    public void InstantiatePlayer(int id)
    {
        CharacterBody3D player = playerScene.Instantiate() as CharacterBody3D;
        player.Name = id.ToString();
        player.SetMultiplayerAuthority(id);
        AddChild(player);
        player.GlobalPosition = player.GlobalPosition with { Y = 5 };
    }
}