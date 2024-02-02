using Godot;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public partial class ClientUDP : Node
{
    UdpClient udpClient; // UDP Client

    Label StatusLabel;

    //public bool isConnected = false;

    PlayersManager playersManager;

    PacketProcessing packetProcessing = new PacketProcessing();// Object that deals with packets



    public override void _Ready()
    {
        // init
        StatusLabel = GetParent().GetChild<Label>(1);
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        // end
        StatusLabel.Text = "Not connected to server";
    }

    public bool Connect(string ip, int port)
    {
        try
        {
            udpClient = new UdpClient();
            udpClient.Connect(ip, port);
            GD.Print("Requesting connection to the server...");

            // Send message that i want to connect
            int commandType = 1; // Type 1 means wanting to connect to the server
            byte[] sentMessageByte = Encoding.ASCII.GetBytes($"#{commandType}#");
            udpClient.Send(sentMessageByte, sentMessageByte.Length);

            // Await response if connection was accepted
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.Receive(ref serverEndPoint);
            string message = Encoding.ASCII.GetString(receivedBytes);

            int.TryParse(message, out int answer);

            if (answer == 1)
            {
                //isConnected = true;
                GD.Print("Connection was successful");
                return true;
            }
            else
            {
                GD.Print("Connection failed");
                StatusLabel.Text = "Server is full";
                return false;
            }
        }
        catch (Exception ex)
        {
            GD.Print($"Failed to connect, exception: {ex}");
            StatusLabel.Text = "Failed to connect";
            return false;
        }
    }

    public int Authentication(bool LoginOrRegister, string username, string password)
    {
        GD.Print("Authentication started...");
        PasswordHasher passwordHasher = new PasswordHasher();
        string hashedPassword = passwordHasher.HashPassword(password + "secretxd");

        // Sends username / pass to server
        LoginData loginData = new LoginData
        {
            lr = LoginOrRegister,
            un = username,
            pw = hashedPassword
        };

        int receivedCode = -1;
        try
        {
            string jsonData = JsonSerializer.Serialize(loginData, LoginDataContext.Default.LoginData);
            int commandType = 2; // Type 2 means user wants to login/register
            byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#{jsonData}");
            udpClient.Send(messageByte, messageByte.Length);
            GD.Print("Sent login data");

            // Waits for reply if login/register was successful
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.Receive(ref serverEndPoint);
            string receivedData = Encoding.ASCII.GetString(receivedBytes);
            GD.Print(receivedData);
            int.TryParse(receivedData, out receivedCode);
            //GD.Print(receivedCode);
            GD.Print("Receive answer about login");
        }
        catch // Runs if there is no connection to the server
        {
            GD.Print("fail");
            return -1;
        }
        if (LoginOrRegister == true) // Runs if wanting to login
        {
            if (receivedCode == 1) // Login successful
            {
                playersManager.SpawnPlayer();
                AuthenticationSuccessful();
                return 1;
            }
            else // Possibly wrong username or password
            {
                return 0;
            }
        }
        else // Runs if wanting to register
        {
            if (receivedCode == 1) // Registration successful
            {
                playersManager.SpawnPlayer();
                AuthenticationSuccessful();
                return 1;
            }
            else if (receivedCode == 2) // Username is longer than 16 characters
            {
                return 2;
            }
            else if (receivedCode == 3) // Username is already taken
            {
                return 3;
            }
            else // Unknown error
            {
                return 0;
            }
        }
    }
    void AuthenticationSuccessful()
    {
        GD.Print("Authentication successful, waiting for server to send initial data");
        StatusLabel.Text = "Authentication successful, waiting for server to send initial data";

        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.Receive(ref serverEndPoint);
            string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length);

            InitialData initialData = JsonSerializer.Deserialize(receivedData, InitialDataContext.Default.InitialData);

            playersManager.PreSpawnPuppets(initialData.i, initialData.mp); // Sets the max amount of players the server can have and sets the index so the puppet of the local player wont be visible

            Task.Run(() => ReceiveDataUDP());
            Task.Run(() => SendDataUDP());

            GD.Print("Initial data received, connected");
            StatusLabel.Text = "Connected";
        }
        catch (Exception ex)
        {
            GD.Print(ex);
            return;
        }

    }
    async Task ReceiveDataUDP()
    {
        while (true)
        {
            try
            {
                UdpReceiveResult udpReceiveResult = await udpClient.ReceiveAsync();

                Packet packet = packetProcessing.BreakUpPacket(udpReceiveResult.Buffer);

                //GD.Print($"Received packet type: {packet.type}, data: {packet.data}");

                switch (packet.type)
                {
                    case 0: // Server is pinging the client
                        GD.Print("Server sent a ping");
                        int commandType = 0; // Type 0 means client answers the server's ping
                        byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#");
                        await udpClient.SendAsync(messageByte, messageByte.Length);
                        break;
                    case 4: // Type 4 means server is sending position of other players
                        playersManager.players = JsonSerializer.Deserialize(packet.data, PlayersContext.Default.Players);
                        playersManager.CallDeferred(nameof(playersManager.ProcessOtherPlayerPosition));
                        break;


                }
            }
            catch
            {
                GD.Print("Error receiving UDP packet");
            }
        }
    }
    async Task SendDataUDP()
    {
        while (true)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerContext.Default.Player);
                int commandType = 3; // Type 3 means client wants to send its position to the server
                byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#{jsonData}");
                await udpClient.SendAsync(messageByte, messageByte.Length);
            }
            catch
            {
                GD.Print("Error sending UDP packet");
            }
            Thread.Sleep(10);
        }
    }
}