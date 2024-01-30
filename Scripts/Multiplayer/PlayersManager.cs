using Godot;


public partial class PlayersManager : Node3D
{
    PackedScene playerScene = GD.Load<PackedScene>("res://Components/Player.tscn");
    PackedScene puppetPlayerScene = GD.Load<PackedScene>("res://Components/PuppetPlayer.tscn");
    TCPClient tcpClient;

    CharacterBody3D playerCharacter; // Player character
    Node3D playerHead; // Player head

    Node3D otherplayers; // Location of "other players" node

    public Players players = new Players(); // Array of every players' position

    public Player localPlayer = new Player(); // Position of local player

    public Vector3[] puppetPositions; // Position of puppet players
    public Vector3[] puppetRotations; // Rotation of puppet players


    public bool interpolatePuppets = true;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        tcpClient = GetNode<TCPClient>("/root/Map/MultiplayerManager/TCPClient");
        otherplayers = GetChild<Node3D>(1);
    }
    public void SpawnPlayer()
    {
        playerCharacter = playerScene.Instantiate() as CharacterBody3D; // Player character
        playerHead = playerCharacter.GetChild<Node3D>(0); // Player head's rotation
        GetChild(0).AddChild(playerCharacter);
        playerCharacter.Position = new Vector3(0f, 3f, 0f);
        SetPhysicsProcess(true);
    }
    public void PreSpawnPuppets(int ownIndex, int maxPlayers)
    {
        puppetPositions = new Vector3[maxPlayers]; // Initializes the vector3 array for puppet positions
        puppetRotations = new Vector3[maxPlayers]; // Initializes the vector3 array for puppet rotations
        players.list = new Player[maxPlayers]; // Initializes the array containing players

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

        System.Numerics.Vector3 sysvec = new System.Numerics.Vector3();
        Godot.Vector3 godvec = new Godot.Vector3(5f, 6f, 4f);

        sysvec.X = godvec.X;
    }
    public override void _PhysicsProcess(double delta)
    {
        PrepareLocalPlayerPositionForSending();
        InterpolatePuppetPlayersPosition((float)delta);
    }
    void PrepareLocalPlayerPositionForSending()
    {
        localPlayer.x = playerCharacter.Position.X;
        localPlayer.y = playerCharacter.Position.Y;
        localPlayer.z = playerCharacter.Position.Z;

        localPlayer.rx = playerHead.Rotation.X;
        localPlayer.ry = playerCharacter.Rotation.Y;
        localPlayer.rz = playerCharacter.Rotation.Z;

        //GD.Print(playerCharacter.Rotation);
    }
    void InterpolatePuppetPlayersPosition(float delta)
    {
        // Interpolation of puppet positions

        float speed = delta * 8;
        int playerCount = otherplayers.GetChildCount();
        for (int i = 0; i < playerCount; i++)
        {
            CharacterBody3D puppet = otherplayers.GetChild<CharacterBody3D>(i);
            Node3D puppetHead = puppet.GetChild<Node3D>(0);

            Vector3 puppetPosition = puppet.Position; // Local position value of the puppet
            Vector3 puppetRotation = puppet.Rotation; // Local rotation value of the puppet
            Vector3 puppetHeadRotation = puppetHead.Rotation; // Local rotation value of the puppet head

            if (interpolatePuppets)
            {
                puppetPosition.X = Mathf.Lerp(puppetPosition.X, puppetPositions[i].X, speed);
                puppetPosition.Y = Mathf.Lerp(puppetPosition.Y, puppetPositions[i].Y, speed);
                puppetPosition.Z = Mathf.Lerp(puppetPosition.Z, puppetPositions[i].Z, speed);

                //puppetRotation.X = Mathf.Lerp(puppetRotation.X, puppetRotations[i].X, speed);
                puppetRotation.Y = Mathf.LerpAngle(puppetRotation.Y, puppetRotations[i].Y, speed); // Rotates the puppet body only on Y angle
                //puppetRotation.Z = Mathf.Lerp(puppetRotation.Z, puppetRotations[i].Z, speed);

                puppetHeadRotation.X = Mathf.LerpAngle(puppetHeadRotation.X, puppetRotations[i].X, speed); // Rotates the puppet head only on X angle
                //puppetHeadRotation.Y = Mathf.Lerp(puppetRotation.Y, puppetRotations[i].Y, speed);
                //puppetHeadRotation.Z = Mathf.Lerp(puppetRotation.Z, puppetRotations[i].Z, speed);
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
        for (int i = 0; i < players.list.Length; i++)
        {

            if (players.list[i] == null) // Runs if player is not found in given slot index
            {
                puppetPositions[i] = new Vector3(0f, -10f, 0f); // Resets puppet player position if not in use
            }
            else // Runs if player is found
            {
                puppetPositions[i] = new Vector3(players.list[i].x, players.list[i].y, players.list[i].z); // Puts the updated position of puppet players in a vector3 array
                puppetRotations[i] = new Vector3(players.list[i].rx, players.list[i].ry, players.list[i].rz); // Puts the updated rotation of puppet players in a vector3 array
            }
        }
    }
}



