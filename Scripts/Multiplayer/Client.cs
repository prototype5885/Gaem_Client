using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;


// using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

public partial class Client : Node
{
    Socket clientTcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    Socket clientUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


    PlayersManager playersManager;
    public bool loginOrRegister; // this is needed so when server sends back response about authentication, the authenticator will know
    GUI gui;
    Hud hud;

    byte connectionStatus = 0;
    bool initialDataReceived = false;

    bool encryption = true;
    static byte[] encryptionKey = Encoding.ASCII.GetBytes("0123456789ABCDEF0123456789ABCDEF");

    // timers for timeout
    const int udpStartTimingOutTime = 2000;
    System.Timers.Timer udpStartTimingOut = new System.Timers.Timer(udpStartTimingOutTime);
    DateTime pingReceivedTime;

    const int udpEndTimingOutTime = 4000;
    System.Timers.Timer udpEndTimingOut = new System.Timers.Timer(udpEndTimingOutTime);

    public override void _Ready()
    {
        // init
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        gui = GetNode<GUI>("/root/Map/GUI");
        hud = GetNode<Hud>("/root/Map/HUD");
        // end
        SetConnectionStatus(0); // sets status text to not connected

        udpStartTimingOut.Elapsed += UdpTimingOut;
        udpStartTimingOut.AutoReset = false;

        udpEndTimingOut.Elapsed += UdpTimedOut;
        udpEndTimingOut.AutoReset = false;
    }
    //public override void _PhysicsProcess(double delta)
    //{
    //    GD.Print(clientTcpSocket.Connected);
    //}

    public async void Connect(string serverIpAddressString, int port, string username, string password)
    {
        try
        {
            if (serverIpAddressString == "localhost") { serverIpAddressString = "127.0.0.1"; }

            IPAddress serverIpAddress = IPAddress.Parse(serverIpAddressString);

            // connects to tcp server
            IPEndPoint serverTcpEndpoint = new IPEndPoint(serverIpAddress, port);
            await clientTcpSocket.ConnectAsync(serverTcpEndpoint);

            // connects to udp server
            await clientUdpSocket.ConnectAsync(serverIpAddress, port + 1);
            IPEndPoint localUdpEndpoint = (IPEndPoint)clientUdpSocket.LocalEndPoint;

            string hashedPassword = String.Empty;
            using (SHA512 sha = SHA512.Create())
            {
                byte[] hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "secretxd"));
                hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
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
            LoginWindow loginWindow = gui.LoginWindow as LoginWindow;
            loginWindow.LoginResult(-1);
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
        SetConnectionStatus(1);

        udpStartTimingOut.Start(); // starts the timer that detects if connection to the server is lost
    }
    async Task ReceiveTcpDataFromServer()
    {
        try
        {
            byte[] buffer = new byte[2048];
            int bytesReceived;
            while (true)
            {

                bytesReceived = await clientTcpSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                ProcessBuffer(buffer, bytesReceived);
            }
        }
        catch
        {
            GD.Print($"Error receiving TCP packet");
        }
    }
    async Task ReceiveUdpDataFromServer()
    {
        try
        {
            byte[] buffer = new byte[8192];
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
            int tickrate = 100;
            while (true)
            {
                Thread.Sleep(tickrate);
                string jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerPositionContext.Default.PlayerPosition);
                await SendTcp(3, jsonData);
            }
        }
        catch
        {
            // GD.Print("Error sending UDP packet");
        }
    }
    async void ProcessBuffer(byte[] buffer, int byteLength)
    {
        try
        {
            //GD.Print(Encoding.ASCII.GetString(buffer, 0, byteLength));
            string receivedBytesInString = string.Empty;
            if (encryption)
            {
                byte[] receivedBytes = new byte[byteLength];
                Array.Copy(buffer, receivedBytes, byteLength);

                receivedBytesInString = Encryption.Decrypt(receivedBytes, encryptionKey);
            }
            else
            {
                receivedBytesInString = Encoding.ASCII.GetString(buffer, 0, byteLength);
            }
            //GD.Print(receivedBytesInString);
            string packetTypePattern = @"#(.*)#";
            string packetDataPattern = @"\$(.*?)\$";

            MatchCollection packetTypeMatches = Regex.Matches(receivedBytesInString, packetTypePattern);
            MatchCollection packetDataMatches = Regex.Matches(receivedBytesInString, packetDataPattern);

            for (int i = 0; i < packetTypeMatches.Count; i++)
            {
                int.TryParse(packetTypeMatches[i].Groups[1].Value, out int typeOfPacket);

                Packet packet = new Packet();
                packet.type = typeOfPacket;
                packet.data = packetDataMatches[i].Groups[1].Value;

                await ProcessDataSentByServer(packet);
            }
        }
        catch
        {
            GD.Print("Error processing packet");
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
                    if (connectionStatus != 1)
                        SetConnectionStatus(1); // sets connection status text to connected, if its not already

                    hud.PingReceived(); // indicates on the screen for 250 ms that ping request has been received from the server

                    CalculateLatency();
                    pingReceivedTime = DateTime.UtcNow;

                    udpStartTimingOut.Interval = udpStartTimingOutTime;
                    udpEndTimingOut.Interval = udpEndTimingOutTime;
                    udpEndTimingOut.Stop();

                    await SendTcp(0, "");
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
    public async Task SendTcp(byte commandType, string message)
    {
        try
        {
            byte[] messageBytes = EncodeMessage(commandType, message);
            await clientTcpSocket.SendAsync(messageBytes, SocketFlags.None);
            GD.Print($"Message {message} was sent successfully");

        }
        catch
        {
            //GD.Print($"Error sending TCP message type {commandType}.");
            GD.Print($"Failed to send message {message}");
        }
    }
    public async Task SendUdp(byte commandType, string message)
    {
        try
        {
            byte[] messageBytes = EncodeMessage(commandType, message);
            //GD.Print("UDP message length: " + messageBytes.Length);
            await clientUdpSocket.SendAsync(messageBytes, SocketFlags.None);
        }
        catch
        {
            Console.WriteLine($"Error sending UDP message type {commandType}.");
        }
    }
    byte[] EncodeMessage(byte commandType, string message)
    {
        if (encryption)
        {
            return Encryption.Encrypt($"#{commandType}#${message}$", encryptionKey); // encodes the message encrypted
        }
        else
        {
            return Encoding.ASCII.GetBytes($"#{commandType}#${message}$"); // encodes the message
        }
    }
    void SetConnectionStatus(byte newStatus)
    {
        connectionStatus = newStatus;
        hud.CallDeferred(nameof(hud.SetConnectionStatusText), connectionStatus);
    }
    void UdpTimingOut(object sender, ElapsedEventArgs e)
    {
        GD.Print("Started timing out");
        SetConnectionStatus(2);
        udpEndTimingOut.Start();
    }
    void UdpTimedOut(object sender, ElapsedEventArgs e)
    {
        GD.Print("Udp timed out");
        SetConnectionStatus(3);
    }
    void CalculateLatency()
    {
        TimeSpan timespan = pingReceivedTime - DateTime.UtcNow;
        int latency = Math.Abs(timespan.Milliseconds) / 2;
        //GD.Print(latency);
        hud.CallDeferred(nameof(hud.UpdateLatencyOnHud), latency);
    }
}