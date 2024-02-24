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
    readonly Socket clientTcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    readonly Socket clientUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    Label StatusLabel;
    PlayersManager playersManager;
    public bool loginOrRegister; // this is needed so when server sends back response about authentication, the authenticator will know
    GUI gui;
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
    }

    public async void Connect(string serverIpAddressString, int port, string username, string password)
    {
        try
        {
            if (serverIpAddressString == "localhost") { serverIpAddressString = "127.0.0.1"; }

            IPAddress serverIpAddress = IPAddress.Parse(serverIpAddressString);

            // connects to tcp server
            IPEndPoint serverTcpEndpoint = new IPEndPoint(serverIpAddress, port);
            clientTcpSocket.Connect(serverTcpEndpoint);

            // connects to udp server
            clientUdpSocket.Connect(serverIpAddress, port + 1);
            IPEndPoint localUdpEndpoint = (IPEndPoint)clientUdpSocket.LocalEndPoint;

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
            await SendTcp(1, jsonData);

            GD.Print("Sent login data to the server");

            await ReceiveTcpDataFromServer(); // starts receiving data from the server, this will stay on for as long as the client is connected to the server
        }
        catch (Exception ex) // Runs if there is no connection to the server
        {
            GD.Print(ex);
            //GD.Print("Failed to connect");
        }
    }

    void AuthenticationSuccessful(int clientIndex, int maxPlayers)
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
    async Task ReceiveTcpDataFromServer()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesReceived;
            while (true)
            {
                bytesReceived = await clientTcpSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                ProcessBuffer(buffer, bytesReceived);
            }
        }
        catch
        {
            GD.Print("Error receiving TCP packet");
        }
    }
    async Task ReceiveUdpDataFromServer()
    {
        try
        {
            byte[] buffer = new byte[4096];
            int bytesReceived;
            while (true)
            {
                bytesReceived = await clientUdpSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                ProcessBuffer(buffer, bytesReceived);
            }
        }
        catch
        {
            GD.Print("Error receiving UDP packet");
        }
    }
    async Task SendPositionToServer()
    {
        try
        {
            string jsonData;
            while (true)
            {
                Thread.Sleep(tickrate);
                jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerPositionContext.Default.PlayerPosition);
                await SendUdp(3, jsonData);
            }
        }
        catch
        {
            // GD.Print("Error sending UDP packet");
        }
    }
    void SetConnectionStatusText(byte newStatus)
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

    async void ProcessBuffer(byte[] receivedBytes, int byteLength)
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
    async Task ProcessDataSentByServer(Packet packet)
    {
        try
        {
            switch (packet.type)
            {
                // Server is pinging the client
                case 0:
                    if (connectionStatus != 1) CallDeferred(nameof(SetConnectionStatusText), 1); // sets connection status text to connected, if its not already
                    await SendUdp(0, "");
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
                    GD.Print(packet.data);
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
    async Task SendTcp(byte commandType, string message)
    {
        try
        {
            byte[] messageBytes = EncodeMessage(commandType, message);
            await clientTcpSocket.SendAsync(messageBytes, SocketFlags.None);
        }
        catch
        {
            Console.WriteLine($"Error sending TCP message type {commandType}.");
        }
    }
    async Task SendUdp(byte commandType, string message)
    {
        try
        {
            byte[] messageBytes = EncodeMessage(commandType, message);
            await clientUdpSocket.SendAsync(messageBytes, SocketFlags.None);
        }
        catch
        {
            Console.WriteLine($"Error sending UDP message type {commandType}.");
        }
    }
    byte[] EncodeMessage(byte commandType, string message)
    {
        return Encoding.ASCII.GetBytes($"#{commandType}#${message}$");
    }
}