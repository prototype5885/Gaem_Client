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
    static TcpClient tcpClient;
    public static NetworkStream tcpStream;
    public static Socket clientUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    public static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    static byte connectionStatus = 0;

    public static int tickrate;

    public static async void Connect(string serverIpAddressString, int port, string username, string password)
    {
        try
        {
            if (serverIpAddressString == "localhost") { serverIpAddressString = "127.0.0.1"; }

            IPAddress serverIpAddress = IPAddress.Parse(serverIpAddressString);

            // connects to tcp server
            tcpClient = new TcpClient(serverIpAddressString, port);
            tcpStream = tcpClient.GetStream();

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
                loginOrRegister = ConnectWindows.loginOrRegister,
                username = username,
                password = hashedPassword
            };
            string jsonData = JsonSerializer.Serialize(loginData, LoginDataContext.Default.LoginData);
            await PacketProcessor.SendTcp(1, jsonData);

            GD.Print("Sent login data to the server");

            await PacketProcessor.ReceiveTcpData(); // starts receiving tcp data from the server, this will stay on for as long as the client is connected to the server
        }
        catch// Runs if there is no connection to the server
        {
            GD.Print("Failed to connect to server");
            ConnectWindows.ConnectionResult(0);
        }
    }

    public static void AuthenticationSuccessful(int clientIndex, int maxPlayers)
    {
        GD.Print("Authentication successful, waiting for server to send initial data");

        NodeManager.playersManager.CallDeferred(nameof(NodeManager.playersManager.SpawnPlayer));
        NodeManager.playersManager.CallDeferred(nameof(NodeManager.playersManager.PreSpawnPuppets), clientIndex, maxPlayers);

        Task.Run(() => PlayersManager.SendPositionToServer());
        Task.Run(() => PacketProcessor.ReceiveUdpData());

        GD.Print("Initial data received, connected");
        SetConnectionStatus(1);
    }
    static void SetConnectionStatus(byte newStatus)
    {
        connectionStatus = newStatus;
        NodeManager.hud.CallDeferred(nameof(NodeManager.hud.SetConnectionStatusText), connectionStatus);
    }
}