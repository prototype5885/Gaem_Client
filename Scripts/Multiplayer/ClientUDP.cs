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
    private UdpClient udpClient; // UDP Client

    private Label StatusLabel;

    //public bool isConnected = false;

    private PlayersManager playersManager;

    private static readonly PacketProcessing packetProcessing = new PacketProcessing();// Object that deals with packet

    //public string serverAddress = string.Empty;
    //public int serverPort = 0;

    public bool loginOrRegister; // this is needed so when server sends back response about authentication, the authenticator will know

    private GUI gui;


    public override void _Ready()
    {
        // init
        StatusLabel = GetParent().GetChild<Label>(1);
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        gui = GetNode<GUI>("/root/Map/GUI");
        // end
        StatusLabel.Text = "Not connected to server";
    }

    public void Connect(string serverAddress, int port, string username, string password)
    {
        try
        {
            if (serverAddress == "localhost") { serverAddress = "127.0.0.1"; }

            udpClient = new UdpClient();
            udpClient.Connect(serverAddress, port);

            PasswordHasher passwordHasher = new PasswordHasher();
            string hashedPassword = passwordHasher.HashPassword(password + "secretxd");

            // Sends username / pass to server
            LoginData loginData = new LoginData
            {
                lr = loginOrRegister,
                un = username,
                pw = hashedPassword
            };
            string jsonData = JsonSerializer.Serialize(loginData, LoginDataContext.Default.LoginData);
            byte commandType = 1; // Type 1 means user wants to login/register
            byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#{jsonData}");
            udpClient.Send(messageByte, messageByte.Length);
            GD.Print("Sent login data to the server");


            Task.Run(() => ReceiveDataFromServer());
        }
        catch // Runs if there is no connection to the server
        {
            GD.Print("Failed to connect");
        }
    }

    private void AuthenticationSuccessful(int clientIndex, int maxPlayers)
    {
        GD.Print("Authentication successful, waiting for server to send initial data");
        StatusLabel.Text = "Authentication successful, waiting for server to send initial data";

        // IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
        // byte[] receivedBytes = udpClient.Receive(ref serverEndPoint);
        // string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length);
        //
        // InitialData initialData = JsonSerializer.Deserialize(receivedData, InitialDataContext.Default.InitialData);
        playersManager.SpawnPlayer();
        playersManager.PreSpawnPuppets(clientIndex, maxPlayers); // Sets the max amount of players the server can have and sets the index so the puppet of the local player wont be visible

        Task.Run(() => SendDataToServer());

        GD.Print("Initial data received, connected");
        StatusLabel.Text = "Connected";


    }
    private async Task ReceiveDataFromServer()
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
                        CallDeferred(nameof(PingReceived));
                        byte commandType = 0; // Type 0 means client answers the server's ping
                        byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#");
                        await udpClient.SendAsync(messageByte, messageByte.Length);
                        break;
                    case 1:
                        GD.Print("Server responded to login");
                        int.TryParse(packet.data, out int receivedCode); // Type 1 means server responded to login/registering

                        // LoginWindow loginWindow = GetNode<LoginWindow>("/root/Map/GUI/JoinWindows/LoginWindow");
                        // RegistrationWindow registrationWindow = GetNode<RegistrationWindow>("/root/Map/GUI/JoinWindows/RegistrationWindow");
                        LoginWindow loginWindow = gui.LoginWindow as LoginWindow;
                        RegistrationWindow registrationWindow = gui.RegistrationWindow as RegistrationWindow;

                        if (loginOrRegister == true) // Runs if wanting to login
                        {
                            loginWindow.CallDeferred(nameof(loginWindow.LoginResult), receivedCode);
                        }
                        else // Runs if wanting to register
                        {
                            registrationWindow.CallDeferred(nameof(registrationWindow.RegistrationResult), receivedCode);
                        }
                        break;
                    case 2: // Type 2 means server sent the initial data to the client
                        InitialData initialData = JsonSerializer.Deserialize(packet.data, InitialDataContext.Default.InitialData);
                        CallDeferred(nameof(AuthenticationSuccessful), initialData.i, initialData.mp);
                        break;
                    case 3: // Type 3 means server is sending position of other players
                        playersManager.everyPlayersPosition = JsonSerializer.Deserialize(packet.data, EveryPlayersPositionContext.Default.EveryPlayersPosition);
                        playersManager.CallDeferred(nameof(playersManager.ProcessOtherPlayerPosition));
                        break;
                    case 4:
                        EveryPlayersName everyPlayersName = JsonSerializer.Deserialize(packet.data, EveryPlayersNameContext.Default.EveryPlayersName);
                        break;
                }
            }
            catch
            {
                GD.Print("Error receiving UDP packet");
            }
        }
    }
    async Task SendDataToServer()
    {
        while (true)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerPositionContext.Default.PlayerPosition);
                byte commandType = 3; // Type 3 means client wants to send its position to the server
                // GD.Print(jsonData);
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
    void PingReceived()
    {
        StatusLabel.Text = "Connected";
    }
}