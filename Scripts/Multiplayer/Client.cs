using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
    // tcp stuff
    private TcpClient serverConnection;
    private NetworkStream serverStream;
    private IPEndPoint serverTcpEndpoint;

    // udp stuff
    private Socket clientUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    //private EndPoint serverUdpEndpoint;

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

    int tickrate = 10;

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

            serverConnection = new TcpClient(serverAddress, port); // connects to tcp server
            serverStream = serverConnection.GetStream();

            serverTcpEndpoint = (IPEndPoint)serverConnection.Client.RemoteEndPoint;

            clientUdpSocket.Connect(IPAddress.Parse(serverAddress), port + 1); // connects to udp server
            //IPEndPoint localUdpEndpoint = (IPEndPoint)clientUdpSocket.LocalEndPoint;

            string hashedPassword = "";
            using (var passwordHasher = new PasswordHasher())
            {
                hashedPassword = passwordHasher.HashPassword(password + "secretxd");
            }

            // Sends username / pass to server
            LoginData loginData = new LoginData
            {
                loginOrRegister = loginOrRegister,
                username = username,
                password = hashedPassword,
                //udpPort = localUdpEndpoint.Port
            };
            string jsonData = JsonSerializer.Serialize(loginData, LoginDataContext.Default.LoginData);
            await Send(1, jsonData, true);

            GD.Print("Sent login data to the server");

            await ReceiveTcpDataFromServer(); // starts receiving data from the server, this will stay on for as long as the client is connected to the server
        }
        catch (Exception ex) // Runs if there is no connection to the server
        {
            GD.Print(ex);
            //GD.Print("Failed to connect");
        }
    }

    private void AuthenticationSuccessful(int clientIndex, int maxPlayers)
    {
        // GD.Print("Authentication successful, waiting for server to send initial data");
        // StatusLabel.Text = "Authentication successful, waiting for server to send initial data";

        playersManager.SpawnPlayer();
        playersManager.PreSpawnPuppets(clientIndex, maxPlayers); // Sets the max amount of players the server can have and sets the index so the puppet of the local player wont be visible

        Task.Run(() => SendPositionToServer());
        Task.Run(() => ReceiveUdpDataFromServer());

        GD.Print("Initial data received, connected");
        SetConnectionStatusText(1);
    }
    private async Task ReceiveTcpDataFromServer()
    {

        while (true)
        {
            try
            {
                byte[] buffer = new byte[8192];
                int receivedBytes = await serverStream.ReadAsync(buffer, 0, buffer.Length);
                //GD.Print(receivedBytes);
                ProcessBuffer(buffer, receivedBytes);
            }
            catch
            {
                GD.Print("Error receiving UDP packet");
            }
        }
    }
    private async Task ReceiveUdpDataFromServer()
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[4096];

                int receivedBytes = await clientUdpSocket.ReceiveAsync(buffer, SocketFlags.None);

                GD.Print("receivedudp");


                ProcessBuffer(buffer, receivedBytes);

            }
        }
        catch
        {

        }
    }
    private async Task SendPositionToServer()
    {
        while (true)
        {
            try
            {
                Thread.Sleep(tickrate);
                string jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerPositionContext.Default.PlayerPosition);
                await Send(3, jsonData, false);
            }
            catch
            {
                // GD.Print("Error sending UDP packet");
            }

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
    private async Task Send(byte commandType, string message, bool reliable)
    {
        try
        {
            byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#${message}$");

            if (reliable)
            {
                await serverStream.WriteAsync(messageByte);
            }
            else
            {
                await clientUdpSocket.SendAsync(messageByte, SocketFlags.None);
            }
        }
        catch
        {
            // Console.WriteLine($"Error sending message type {commandType}. Exception: {ex.Message}");
        }
    }
    private async void ProcessBuffer(byte[] receivedBytes, int byteLength)
    {
        string packetString = Encoding.ASCII.GetString(receivedBytes, 0, byteLength);


        string packetTypePattern = @"#(.*)#";
        string packetDataPattern = @"\$(.*?)\$";

        MatchCollection packetTypeMatches = Regex.Matches(packetString, packetTypePattern);
        MatchCollection packetDataMatches = Regex.Matches(packetString, packetDataPattern);

        for (int i = 0; i < packetTypeMatches.Count; i++)
        {
            int.TryParse(packetTypeMatches[i].Groups[1].Value, out int typeOfPacket);

            Packet packet = new Packet();
            packet.type = typeOfPacket;
            packet.data = packetDataMatches[i].Groups[1].Value;

            await ProcessDataSentByServer(packet);
        }

    }
    private async Task ProcessDataSentByServer(Packet packet)
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
                    await Send(0, "", false);
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

                    bool result = false;
                    if (loginOrRegister == true) // Runs if wanting to login
                    {
                        result = loginWindow.LoginResult(initialData.lr);
                    }
                    else // Runs if wanting to register
                    {
                        registrationWindow.RegistrationResult(initialData.lr);
                    }

                    if (result == true)
                    {
                        initialDataReceived = true;
                    }
                    // else
                    // {

                    //     initialDataReceived = false;
                    //     serverStream.Close();
                    //     serverConnection = null;
                    // }
                    break;

                case 2:
                    break;

                // Type 3 means server is sending position of other players
                case 3:
                    if (!initialDataReceived) break;

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