using Godot;
using ProToType;
using System;


public partial class PlayersManager : Node3D
{
    PackedScene playerScene = GD.Load<PackedScene>("res://Components/Player.tscn");
    PackedScene puppetPlayerScene = GD.Load<PackedScene>("res://Components/PuppetPlayer.tscn");

    CharacterBody3D playerCharacter; // Player character
    Node3D playerHead; // Player head

    Node3D otherplayers; // Location of "other players" node

    public EveryPlayersPosition everyPlayersPosition = new EveryPlayersPosition(); // Array of every players' position

    public PlayerPosition localPlayer = new PlayerPosition(); // Position of local player

    Vector3[] puppetPositions; // Position of puppet players
    Vector3[] puppetRotations; // Rotation of puppet players

    bool interpolatePuppetPositions = false;
    float interpolationSpeed = 4f;
    //byte roundValue = 7;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        otherplayers = GetChild<Node3D>(1);
    }
    public void SpawnPlayer()
    {
        playerCharacter = playerScene.Instantiate() as CharacterBody3D; // Player character
        playerHead = playerCharacter.GetChild<Node3D>(0); // Player head's rotation
        GetChild(0).AddChild(playerCharacter);
        playerCharacter.Position = new Vector3(0f, 3f, 0f);
        SetPhysicsProcess(true);
        SetProcess(true);
    }
    public void PreSpawnPuppets(int ownIndex, int maxPlayers)
    {
        puppetPositions = new Vector3[maxPlayers]; // Initializes the vector3 array for puppet positions
        puppetRotations = new Vector3[maxPlayers]; // Initializes the vector3 array for puppet rotations
        everyPlayersPosition.p = new PlayerPosition[maxPlayers]; // Initializes the array containing players

        for (int i = 0; i < maxPlayers; i++)
        {
            CharacterBody3D puppet = puppetPlayerScene.Instantiate() as CharacterBody3D;
            otherplayers.AddChild(puppet);
            puppet.Position = new Godot.Vector3(0f, -10f, 0f);

            if (i == ownIndex)
            {
                otherplayers.GetChild<CharacterBody3D>(i).Visible = false;
            }
        }
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        InterpolatePuppetPlayersPosition((float)delta);
        PrepareLocalPlayerPositionForSending();
    }
    void PrepareLocalPlayerPositionForSending()
    {
        //localPlayer.x = (float)Math.Round(playerCharacter.Position.X, roundValue);
        //localPlayer.y = (float)Math.Round(playerCharacter.Position.Y, roundValue);
        //localPlayer.z = (float)Math.Round(playerCharacter.Position.Z, roundValue);

        //localPlayer.rx = (float)Math.Round(playerHead.Rotation.X, roundValue);
        //localPlayer.ry = (float)Math.Round(playerCharacter.Rotation.Y, roundValue);

        localPlayer.x = playerCharacter.Position.X;
        localPlayer.y = playerCharacter.Position.Y;
        localPlayer.z = playerCharacter.Position.Z;

        localPlayer.rx = playerHead.Rotation.X;
        localPlayer.ry = playerCharacter.Rotation.Y;


    }
    void InterpolatePuppetPlayersPosition(float delta)
    {
        // Interpolation of puppet positions
        // Vector3 puppetPosition; // Local position value of the puppet
        // Vector3 puppetRotation; // Local rotation value of the puppet
        // Vector3 puppetHeadRotation; // Local rotation value of the puppet head

        CharacterBody3D puppet;
        Node3D puppetHead;

        float speed = delta * interpolationSpeed;
        int playerCount = otherplayers.GetChildCount();
        for (int i = 0; i < playerCount; i++)
        {
            puppet = otherplayers.GetChild<CharacterBody3D>(i);
            puppetHead = puppet.GetChild<Node3D>(0);

            Vector3 puppetPosition = puppet.Position;
            Vector3 puppetRotation = puppet.Rotation;
            Vector3 puppetHeadRotation = puppetHead.Rotation;

            if (interpolatePuppetPositions)
            {
                puppetPosition.X = Mathf.Lerp(puppetPosition.X, puppetPositions[i].X, speed);
                puppetPosition.Y = Mathf.Lerp(puppetPosition.Y, puppetPositions[i].Y, speed);
                puppetPosition.Z = Mathf.Lerp(puppetPosition.Z, puppetPositions[i].Z, speed);

                puppetRotation.Y = Mathf.LerpAngle(puppetRotation.Y, puppetRotations[i].Y, speed); // Rotates the puppet body only on Y angle
                puppetHeadRotation.X = Mathf.LerpAngle(puppetHeadRotation.X, puppetRotations[i].X, speed); // Rotates the puppet head only on X angle
            }
            else
            {
                puppetPosition = puppetPositions[i]; // Position of the puppet
                puppetRotation.Y = puppetRotations[i].Y; // Rotates the puppet body only on Y angle
                puppetHeadRotation.X = puppetRotations[i].X; // Rotates the puppet head only on X angle
            }

            puppet.Position = puppetPosition; // Sets the position of the puppet 
            puppet.Rotation = puppetRotation; // Sets the final rotation of the puppet
            puppetHead.Rotation = puppetHeadRotation; // Sets the final rotation of the puppet head
        }
    }
    public void ProcessOtherPlayerPosition()
    {
        for (int i = 0; i < everyPlayersPosition.p.Length; i++)
        {
            if (everyPlayersPosition.p[i] == null) // Runs if player is not found in given slot index
            {

                puppetPositions[i] = new Vector3(0f, -10f, 0f); // Resets puppet player position if not in use
            }
            else // Runs if player is found
            {

                puppetPositions[i] = new Vector3(everyPlayersPosition.p[i].x, everyPlayersPosition.p[i].y, everyPlayersPosition.p[i].z); // Puts the updated position of puppet players in a vector3 array
                puppetRotations[i] = new Vector3(everyPlayersPosition.p[i].rx, everyPlayersPosition.p[i].ry, 0f); // Puts the updated rotation of puppet players in a vector3 array

            }
        }
    }
    public void ProcessOtherPlayerName()
    {

    }
}



