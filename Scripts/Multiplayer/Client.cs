using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

// using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

public partial class Client : Node
{
    private TcpClient serverConnection;
    private NetworkStream serverStream;

    private Label StatusLabel;

    //public bool isConnected = false;

    private PlayersManager playersManager;


    //public string serverAddress = string.Empty;
    //public int serverPort = 0;

    public bool loginOrRegister; // this is needed so when server sends back response about authentication, the authenticator will know

    private GUI gui;

    private static System.Timers.Timer timeoutTimer = new System.Timers.Timer(1000);

    byte connectionStatus = 0;

    bool initialDataReceived = false;

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

            serverConnection = new TcpClient(serverAddress, port);
            serverStream = serverConnection.GetStream();

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
            await Send(1, jsonData, serverStream);

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
                byte[] buffer = new byte[8192];
                int receivedBytes = await serverStream.ReadAsync(buffer, 0, buffer.Length);
                BreakUpPacket(buffer, receivedBytes);
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
                await Send(3, jsonData, serverStream);
            }
            catch
            {
                // GD.Print("Error sending UDP packet");
            }
            Thread.Sleep(100);
        }
    }
    private void ServerTimedOutEvent(Object source, ElapsedEventArgs e)
    {
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
    private async Task Send(byte commandType, string message, NetworkStream stream)
    {
        try
        {
            byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#{message}");

            await stream.WriteAsync(messageByte);
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Error sending message type {commandType}. Exception: {ex.Message}");
        }
    }
    private async void BreakUpPacket(byte[] receivedBytes, int byteLength)
    {
        string packetString = Encoding.ASCII.GetString(receivedBytes, 0, byteLength);

        string packetTypePattern = @"#(.*)#";
        string packetDataPattern = @"\$(.*?)\$";

        MatchCollection packetTypeMatches = Regex.Matches(packetString, packetTypePattern);
        MatchCollection packetDataMatches = Regex.Matches(packetString, packetDataPattern);

        List<Packet> packets = new List<Packet>();
        for (int i = 0; i < packetTypeMatches.Count; i++)
        {
            int.TryParse(packetTypeMatches[i].Groups[1].Value, out int typeOfPacket);

            Packet packet = new Packet();
            packet.type = typeOfPacket;

            // GD.Print(packet.type);

            packet.data = packetDataMatches[i].Groups[1].Value;
            // GD.Print("Processed:" + packet.data);

            packets.Add(packet);


        }

        // if (packets.Count > 1)
        // {
        //     GD.Print("Multiple");
        // }

        foreach (Packet packet in packets)
        {
            await ProcessPackets(packet);

        }
    }
    private async Task ProcessPackets(Packet packet)
    {
        try
        {
            switch (packet.type)
            {
                // Server is pinging the client
                case 0:
                    // GD.Print("Server sent a ping");
                    ResetTimeoutTimer();
                    if (connectionStatus != 1) CallDeferred(nameof(SetConnectionStatusText), 1); // sets connection status text to connected, if its not already
                    await Send(0, "", serverStream);
                    break;
                // Type 1 means server is responding to login/registering
                case 1:
                    InitialData initialData = JsonSerializer.Deserialize(packet.data, InitialDataContext.Default.InitialData);
                    if (initialData.lr == 1)
                    {
                        CallDeferred(nameof(AuthenticationSuccessful), initialData.i, initialData.mp);
                    }
                    GD.Print("Server responded to login");
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
            }
        }
        catch
        {
            GD.Print("Packet error");
        }
    }
}