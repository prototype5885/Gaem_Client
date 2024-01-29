using Godot;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using static Godot.Projection;

public partial class PlayersManager : Node3D
{
    PackedScene playerScene = GD.Load<PackedScene>("res://Components/Player.tscn");
    PackedScene puppetPlayerScene = GD.Load<PackedScene>("res://Components/PuppetPlayer.tscn");
    TCPClient tcpClient;
    CharacterBody3D playerCharacter;

    Node3D otherplayers;

    public Players players = new Players();

    public Vector3[] puppetPositions;

    public bool interpolatePuppets = true;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        tcpClient = GetNode<TCPClient>("/root/Map/MultiplayerManager/TCPClient");
        otherplayers = GetChild<Node3D>(1);

    }
    public void SpawnPlayer()
    {
        playerCharacter = playerScene.Instantiate() as CharacterBody3D;
        GetChild(0).AddChild(playerCharacter);
        playerCharacter.Position = new Vector3(0f, 3f, 0f);
        SetPhysicsProcess(true);
    }
    public void PreSpawnPuppets(int ownIndex, int maxPlayers)
    {
        puppetPositions = new Vector3[maxPlayers];
        players.list = new Player[maxPlayers];
        for (int i = 0; i < maxPlayers; i++)
        {
            Node3D puppet = puppetPlayerScene.Instantiate() as Node3D;
            otherplayers.AddChild(puppet);
            puppet.Position = new Vector3(0f, -10f, 0f);

            if (i == ownIndex)
            {
                otherplayers.GetChild<Node3D>(i).Visible = false;
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        tcpClient.localPlayer.x = playerCharacter.Position.X;
        tcpClient.localPlayer.y = playerCharacter.Position.Y;
        tcpClient.localPlayer.z = playerCharacter.Position.Z;

        // Interpolation of puppet positions
        if (interpolatePuppets)
        {
            float speed = (float)delta * 16;
            int playerCount = otherplayers.GetChildCount();
            for (int i = 0; i < playerCount; i++)
            {
                Node3D puppet = otherplayers.GetChild<Node3D>(i);
                Vector3 position = puppet.Position;

                position.X = Mathf.Lerp(position.X, puppetPositions[i].X, speed);
                position.Y = Mathf.Lerp(position.Y, puppetPositions[i].Y, speed);
                position.Z = Mathf.Lerp(position.Z, puppetPositions[i].Z, speed);

                puppet.Position = position;
            }
        }
        else
        {
            int playerCount = otherplayers.GetChildCount();
            for (int i = 0; i < playerCount; i++)
            {
                Node3D puppet = otherplayers.GetChild<Node3D>(i);
                Vector3 position = puppet.Position;

                position = puppetPositions[i];

                puppet.Position = position;
            }
        }


    }

    public void ProcessOtherPlayerPosition()
    {
        for (int i = 0; i < players.list.Length; i++)
        {

            if (players.list[i] == null)
            {
                puppetPositions[i] = new Vector3(0f, -10f, 0f);
                //Vector3 position = new Vector3(0f, -10f, 0f);
                //Node3D puppet = otherplayers.GetChild<Node3D>(i);
                //puppet.Position = position;
            }
            else
            {
                puppetPositions[i] = new Vector3(players.list[i].x, players.list[i].y, players.list[i].z);
                //Vector3 position = new Vector3(players.list[i].x, players.list[i].y, players.list[i].z);
                //Node3D puppet = otherplayers.GetChild<Node3D>(i);
                //puppet.Position = position;
            }
            //GD.Print(players.list[i]);
        }
    }
}



