using Godot;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ProToTypeLounge.Scripts.Multiplayer;

public partial class Client : Node
{
    private static TcpClient tcpClient;
    public static NetworkStream tcpStream;
    public static readonly Socket clientUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    public static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public static Player[] players;

    private static byte connectionStatus;

    public static int tickRate;
    public static byte maxPlayers;
    public static byte ownIndex;

    private static bool receivedInitialData;

    public override void _Ready()
    {
        SetProcess(false);
    }
    
    public static async void Connect(string serverIpAddressString, int port, string username, string password)
    {
        // Connect or register button is pressed here
        try
        {
            if (serverIpAddressString == "localhost") { serverIpAddressString = "127.0.0.1"; }

            IPAddress serverIpAddress = IPAddress.Parse(serverIpAddressString);

            // connects to tcp server
            tcpClient = new TcpClient(serverIpAddressString, port);
            tcpStream = tcpClient.GetStream();

            // connects to udp server
            await clientUdpSocket.ConnectAsync(serverIpAddress, port + 1);
            
            byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            // Sends username / pass to server
            string jsonData = JsonSerializer.Serialize
            (
                new LoginData
                {
                    reg = ConnectWindows.wantingToRegister,
                    un = username,
                    pw = hashedPassword
                }
            );
            await SendPacket.SendTcp(1, jsonData);

            GD.Print("Sent login data to the server");

            await ReceivePacket.ReceiveTcpData(); // starts receiving tcp data from the server, this will stay on for as long as the client is connected to the server
        }
        catch // Runs if there is no connection to the server
        {
            GD.Print("Failed to connect to server");
            ConnectWindows.ConnectionResult(0);
        }
    }

    public static void Authentication(InitialData initialData) // runs when player received the initial data from the server after connection
    {
        if (receivedInitialData) return; // stops if for some reason server would send initial data multiple times

        if (ConnectWindows.ConnectionResult(initialData.rv)) // runs if login was successful
        {
            GD.Print("Authentication successful");

            tickRate = initialData.tr;
            maxPlayers = initialData.mp;
            ownIndex = initialData.i;
            players = new Player[maxPlayers];

            NodeManager.playersManager.UpdateDataOfEveryPlayers(initialData.pda);

            Task.Run(PlayersManager.SendPositionToServer);
            GD.Print("Sending position to the server");
            Task.Run(ReceivePacket.ReceiveUdpData);
            GD.Print("Started ReceiveUdpData");

            receivedInitialData = true;
            SetConnectionStatus(1);
        }
    }

    public static void PlayerDataArrived(PlayerData[] playerDataArray)
    {
        NodeManager.playersManager.UpdateDataOfEveryPlayers(playerDataArray);
    }
    private static void SetConnectionStatus(byte newStatus)
    {
        connectionStatus = newStatus;
        NodeManager.hud.CallDeferred(nameof(NodeManager.hud.SetConnectionStatusText), connectionStatus);
    }
}