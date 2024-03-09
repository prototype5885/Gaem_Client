using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

public partial class PlayersManager : Node3D
{
    private static readonly PackedScene playerScene = GD.Load<PackedScene>("res://Components/Player.tscn");
    private static readonly PackedScene puppetPlayerScene = GD.Load<PackedScene>("res://Components/PuppetPlayer.tscn");

    private static bool interpolatePuppetPositions = false;

    private static float interpolationSpeed = 4f;
    
    public override void _Ready()
    {
        SetProcess(false);
    }
    public override void _Process(double delta)
    {
        InterpolatePuppetPlayersPosition((float)delta);
        Client.players[Client.ownIndex].ConvertLocalPositionToServerFormat();
        
        string playersInfo = "";
        foreach (Player player in Client.players)
        {
            if (player == null) playersInfo += "Free\n";
            else playersInfo += player.ToString() + "\n";
            
        }

        NodeManager.playersInfo.Text = playersInfo;
    }
    public void UpdateDataOfEveryPlayers(PlayerData[] playerDataArray)
    {
        for (byte index = 0; index < Client.maxPlayers; index++)
        {
            if (playerDataArray[index] != null && Client.players[index] == null) // runs if slot is occupied on server and no puppet exist locally on that slot
            {
                if (index == Client.ownIndex) // adds player to the array if player is local player
                {
                    CharacterBody3D playerCharacter = playerScene.Instantiate() as CharacterBody3D;
                    AddChild(playerCharacter);
                    
                    Client.players[index] = new Player 
                    {
                        localPlayer = true,
                        playerName = playerDataArray[index].un,
                        body = playerCharacter,
                        head = playerCharacter.GetChild<Node3D>(0),
                    }; 
                    Client.players[index].body.Position = new Godot.Vector3(0f, 3f, 0f); // positions above the map
                    GD.Print($"Added local player to player array, index {index}");
                    SetProcess(true);
                }
                else // adds player to the array if player is other player
                {
                    GD.Print("Added puppet player to player array");
                    CharacterBody3D playerCharacter = puppetPlayerScene.Instantiate() as CharacterBody3D;
                    AddChild(playerCharacter);

                    Client.players[index] = new Player
                    {
                        localPlayer = false,
                        playerName = playerDataArray[index].un,
                        body = playerCharacter,
                        head = playerCharacter.GetChild<Node3D>(0),
                        nameIndicator = playerCharacter.GetNode<Label3D>("Name")
                    };
                    Client.players[index].UpdateNameIndicator();
                    Client.players[index].body.Position = new Godot.Vector3(0f, 3f, 0f); // positions above the map
                }
            }
            else if (playerDataArray[index] == null) // runs if slot on server is empty but a puppet exists locally
            {
                GD.Print($"Deleted player index {index} from player array");
                Client.players[index] = null;
            }
        }
        GD.Print("Ran UpdateDataOfEveryPlayers");
    }
    public static void UpdateOtherPlayersPosition(PlayerPosition[] everyPlayersPosition) // updates the position of players when received data from server
    {
        for (byte i = 0; i < Client.maxPlayers; i++)
        {
            if (Client.players[i] == null) continue;
            if (i == Client.ownIndex) continue;
            Client.players[i].serverPosition = everyPlayersPosition[i];
        }
    }
    private static void InterpolatePuppetPlayersPosition(float delta) // using the latest position, moves the player puppets to the new positions
    {
        try
        {
            float speed = delta * interpolationSpeed;
            for (int i = 0; i < Client.maxPlayers; i++)
            {
                if (Client.players[i] == null) continue; // skips if no puppet in the slot
                if (Client.players[i].localPlayer) continue; // skips if slot is local player

                PlayerPosition playerPosition = Client.players[i].serverPosition; // assigns to local value, for easier readability
                
                Vector3 puppetPosition = Client.players[i].body.Position; // assigns to local value, for easier readability
                Vector3 puppetRotation = Client.players[i].body.Rotation; // assigns to local value, for easier readability
                Vector3 puppetHeadRotation = Client.players[i].head.Rotation; // assigns to local value, for easier readability

                if (interpolatePuppetPositions) // if interpolation is on
                {
                    puppetPosition.X = Mathf.Lerp(puppetPosition.X, playerPosition.x, speed);
                    puppetPosition.Y = Mathf.Lerp(puppetPosition.Y, playerPosition.y, speed);
                    puppetPosition.Z = Mathf.Lerp(puppetPosition.Z, playerPosition.z, speed);

                    puppetRotation.Y = Mathf.LerpAngle(puppetRotation.Y, playerPosition.ry, speed); // Rotates the puppet body only on Y angle
                    puppetHeadRotation.X = Mathf.LerpAngle(puppetHeadRotation.X, playerPosition.rx, speed); // Rotates the puppet head only on X angle
                }
                else // if interpolation is off
                {
                    puppetPosition = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z); // Position of the puppet
                    puppetRotation.Y = playerPosition.ry; // Rotates the puppet body only on Y angle
                    puppetHeadRotation.X = playerPosition.rx; // Rotates the puppet head only on X angle
                }

                Client.players[i].body.Position = puppetPosition; // Sets the position of the puppet 
                Client.players[i].body.Rotation = puppetRotation; // Sets the final rotation of the puppet
                Client.players[i].head.Rotation = puppetHeadRotation; // Sets the final rotation of the puppet head
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
       
    }
    public static async Task SendPositionToServer()
    {
        try
        {
            while (true)
            {
                Thread.Sleep(Client.tickRate);
                string jsonData = JsonSerializer.Serialize(Client.players[Client.ownIndex].serverPosition);
                await PacketProcessor.SendUdp(3, jsonData);
            }
        }
        catch (Exception ex)
        {
            // GD.Print("Error sending player position to server");
            GD.Print(ex);
        }
    }
}



