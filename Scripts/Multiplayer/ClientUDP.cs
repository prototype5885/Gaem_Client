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
using System.Timers;

public partial class ClientUDP : Node
{
    private Socket socket;

    private Label StatusLabel;

    //public bool isConnected = false;

    private PlayersManager playersManager;

    private static readonly PacketProcessing packetProcessing = new PacketProcessing(); // Object that deals with packet

    //public string serverAddress = string.Empty;
    //public int serverPort = 0;

    public bool loginOrRegister; // this is needed so when server sends back response about authentication, the authenticator will know

    private GUI gui;

    private static System.Timers.Timer timeoutTimer = new System.Timers.Timer(1000);

    byte connectionStatus = 0;

    public override void _Ready()
    {
        // init
        StatusLabel = GetParent().GetChild<Label>(1);
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        gui = GetNode<GUI>("/root/Map/GUI");
        // end
        SetConnectionStatusText(0); // sets status text to not connected

        // set up timer
        timeoutTimer.Elapsed += ServerTimedOutEvent;
        timeoutTimer.Enabled = true;
        // end set up timer

    }

    public async void Connect(string serverAddress, int port, string username, string password)
    {
        try
        {
            if (serverAddress == "localhost") { serverAddress = "127.0.0.1"; }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(serverAddress, port);

            packetProcessing.socket = socket;

            string hashedPassword = "";
            using (var passwordHasher = new PasswordHasher())
            {
                hashedPassword = passwordHasher.HashPassword(password + "secretxd");
            }

            // Sends username / pass to server
            LoginData loginData = new LoginData
            {
                lr = loginOrRegister,
                un = username,
                pw = hashedPassword
            };
            string jsonData = JsonSerializer.Serialize(loginData, LoginDataContext.Default.LoginData);
            await packetProcessing.SendUnreliable(1, jsonData, null);

            GD.Print("Sent login data to the server");

            await ReceiveDataFromServer(); // starts receiving data from the server, this will stay on for as long as the client is connected to the server
        }
        catch // Runs if there is no connection to the server
        {
            GD.Print("Failed to connect");
        }
    }

    private void AuthenticationSuccessful(int clientIndex, int maxPlayers)
    {
        // GD.Print("Authentication successful, waiting for server to send initial data");
        // StatusLabel.Text = "Authentication successful, waiting for server to send initial data";

        playersManager.SpawnPlayer();
        playersManager.PreSpawnPuppets(clientIndex, maxPlayers); // Sets the max amount of players the server can have and sets the index so the puppet of the local player wont be visible

        Task.Run(() => SendPositionToServer());

        GD.Print("Initial data received, connected");
        SetConnectionStatusText(1);
    }
    private async Task ReceiveDataFromServer()
    {
        while (true)
        {
            try
            {
                byte[] buffer = new byte[1024];
                //EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None);

                Packet packet = packetProcessing.BreakUpPacket(buffer, receivedBytes);

                //GD.Print($"Received packet type: {packet.type}, data: {packet.data}");

                switch (packet.type)
                {
                    // Server is pinging the client
                    case 0:
                        GD.Print("Server sent a ping");
                        ResetTimeoutTimer();
                        if (connectionStatus != 1) CallDeferred(nameof(SetConnectionStatusText), 1); // sets connection status text to connected, if its not already
                        await packetProcessing.SendUnreliable(0, "", null);
                        break;
                    // Type 1 means server is responding to login/registering
                    case 1:
                        InitialData initialData = JsonSerializer.Deserialize(packet.data, InitialDataContext.Default.InitialData);
                        if (initialData.lr == 1)
                        {
                            CallDeferred(nameof(AuthenticationSuccessful), initialData.i, initialData.mp);
                        }
                        GD.Print("Server responded to login");
                        //int.TryParse(packet.data, out int receivedCode);
                        // LoginWindow loginWindow = GetNode<LoginWindow>("/root/Map/GUI/JoinWindows/LoginWindow");
                        // RegistrationWindow registrationWindow = GetNode<RegistrationWindow>("/root/Map/GUI/JoinWindows/RegistrationWindow");
                        LoginWindow loginWindow = gui.LoginWindow as LoginWindow;
                        RegistrationWindow registrationWindow = gui.RegistrationWindow as RegistrationWindow;

                        if (loginOrRegister == true) // Runs if wanting to login
                        {
                            loginWindow.CallDeferred(nameof(loginWindow.LoginResult), initialData.lr);
                        }
                        else // Runs if wanting to register
                        {
                            registrationWindow.CallDeferred(nameof(registrationWindow.RegistrationResult), initialData.lr);
                        }
                        break;
                    // Type 2 means server is sending the initial data to the client
                    case 2:

                        break;
                    // Type 3 means server is sending position of other players
                    case 3:
                        playersManager.everyPlayersPosition = JsonSerializer.Deserialize(packet.data, EveryPlayersPositionContext.Default.EveryPlayersPosition);
                        playersManager.CallDeferred(nameof(playersManager.ProcessOtherPlayerPosition));
                        break;
                    case 4:
                        EveryPlayersName everyPlayersName = JsonSerializer.Deserialize(packet.data, EveryPlayersNameContext.Default.EveryPlayersName);
                        break;
                    // Type 10 means its an ACK for a reliable message
                    case 10:
                        packetProcessing.AcknowledgeReceived(packet.data);
                        GD.Print("ack received");
                        break;
                }
            }
            catch
            {
                GD.Print("Error receiving UDP packet");
            }
        }
    }
    private async Task SendPositionToServer()
    {
        while (true)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerPositionContext.Default.PlayerPosition);
                await packetProcessing.SendUnreliable(3, jsonData, null);
            }
            catch
            {
                GD.Print("Error sending UDP packet");
            }
            Thread.Sleep(10);
        }
    }
    private void ServerTimedOutEvent(Object source, ElapsedEventArgs e)
    {
        GD.Print("timeout");
        // SetConnectionStatusText(2);
    }
    private void ResetTimeoutTimer()
    {
        timeoutTimer.Stop();
        timeoutTimer.Interval = 1000;
        timeoutTimer.Start();
    }
    private void SetConnectionStatusText(byte newStatus)
    {
        connectionStatus = newStatus;
        switch (newStatus)
        {
            case 0:
                StatusLabel.Text = "Not connected";
                break;
            case 1:
                StatusLabel.Text = "Connected";
                break;
            case 2:
                StatusLabel.Text = "Timing out";
                break;
        }
    }
}