using Godot;
using System;

public partial class NodeManager : Node3D
{
    public static PlayersManager playersManager;
    public static GUI gui;
    public static Hud hud;

    public override void _Ready()
    {
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        gui = GetNode<GUI>("/root/Map/GUI");
        hud = GetNode<Hud>("/root/Map/HUD");
    }
}
