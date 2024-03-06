//using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;

public static class PacketProcessor
{
    static bool initialDataReceived = false;

    public static async Task SendTcp(byte commandType, string message)
    {
        byte[] messageBytes = EncodeMessage(commandType, message);
        //CalculateLatency.sentBytesPerSecond += messageBytes.Length;
        await Client.tcpStream.WriteAsync(messageBytes);
    }
    public static async Task SendUdp(byte commandType, string message, EndPoint udpEndpoint)
    {
        byte[] messageBytes = EncodeMessage(commandType, message);
        //CalculateLatency.sentBytesPerSecond += messageBytes.Length;
        await Client.clientUdpSocket.SendToAsync(messageBytes, SocketFlags.None, udpEndpoint);
    }
    public static async Task ReceiveTcpData()
    {
        try
        {
            CancellationToken cancellationToken = Client.cancellationTokenSource.Token;

            Byte[] buffer = new Byte[1024];
            int bytesRead;
            while (!cancellationToken.IsCancellationRequested)
            {
                bytesRead = await Client.tcpStream.ReadAsync(new ArraySegment<byte>(buffer), cancellationToken);
                //CalculateLatency.receivedBytesPerSecond += bytesRead;
                Packet[] packets = ProcessBuffer(buffer, bytesRead);
                ProcessPackets(packets);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Receiving task was cancelled");
        }
        catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionReset)
        {
            // Handle sudden client disconnect (ConnectionReset)
            Console.WriteLine($"Client disconnected abruptly");
            //PlayersManager.DisconnectClient(connectedClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    public static async Task ReceiveUdpData()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while (true)
            {
                EndPoint udpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                bytesRead = await Client.clientUdpSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                //CalculateLatency.receivedBytesPerSecond += bytesRead;
                Packet[] packets = ProcessBuffer(buffer, bytesRead);
                ProcessPackets(packets);
            }
        }
        catch
        {
            Console.WriteLine("Error receiving UDP packet");
        }
    }
    public static Packet[] ProcessBuffer(byte[] buffer, int byteLength) // processes bytes received from the buffer
    {
        string receivedBytesInString = string.Empty; // creates this empty string for to be used later

        if (Encryption.encryption) // runs if encryption is enabled
        {
            byte[] receivedBytes = new byte[byteLength];
            Array.Copy(buffer, receivedBytes, byteLength);

            receivedBytesInString = Encryption.Decrypt(receivedBytes);
        }
        else // runs if encryption is disabled
        {
            receivedBytesInString = Encoding.ASCII.GetString(buffer, 0, byteLength);
        }

        //Console.WriteLine(receivedBytesInString);

        string packetTypePattern = @"#(.*)#"; // pattern to read the packet type
        string packetDataPattern = @"\$(.*?)\$"; // pattern to read the packet data

        MatchCollection packetTypeMatches = Regex.Matches(receivedBytesInString, packetTypePattern);
        MatchCollection packetDataMatches = Regex.Matches(receivedBytesInString, packetDataPattern);

        Packet[] packets = new Packet[packetTypeMatches.Count];
        for (byte i = 0; i < packetTypeMatches.Count; i++) // saves all the packets found in the buffer
        {
            byte.TryParse(packetTypeMatches[i].Groups[1].Value, out byte typeOfPacket);

            Packet packet = new Packet();
            packet.type = typeOfPacket;
            packet.data = packetDataMatches[i].Groups[1].Value;

            packets[i] = packet;
        }
        return packets;
    }
    static void ProcessPackets(Packet[] packets)
    {
        foreach (Packet packet in packets)
        {
            ProcessDataSentByServer(packet);
        }
    }
    static void ProcessDataSentByServer(Packet packet)
    {
        try
        {
            switch (packet.type)
            {
                // Server is pinging the client
                case 0:
                    //if (connectionStatus != 1)
                    //SetConnectionStatus(1); // sets connection status text to connected, if its not already

                    //hud.PingReceived(); // indicates on the screen for 250 ms that ping request has been received from the server

                    //CalculateLatency();
                    //pingReceivedTime = DateTime.UtcNow;

                    //udpStartTimingOut.Interval = udpStartTimingOutTime;
                    //udpEndTimingOut.Interval = udpEndTimingOutTime;
                    //udpEndTimingOut.Stop();

                    Task.Run(() => SendTcp(0, ""));
                    break;

                // Type 1 means server is responding to login/registering
                case 1:
                    InitialData initialData = JsonSerializer.Deserialize(packet.data, InitialDataContext.Default.InitialData);

                    if (initialData.lr == 1)
                    {
                        Client.tickrate = initialData.tr;
                        //GetParent().CallDeferred(nameof(Client.AuthenticationSuccessful), initialData.i, initialData.mp);
                        Client.AuthenticationSuccessful(initialData.i, initialData.mp);
                    }
                    //GD.Print("Server responded to login");
                    //LoginWindow loginWindow = NodeManager.gui.LoginWindow as LoginWindow;
                    //RegistrationWindow registrationWindow = NodeManager.gui.RegistrationWindow as RegistrationWindow;

                    bool result = false;
                    if (ConnectWindows.loginOrRegister == true) // Runs if wanting to login
                    {
                        result = ConnectWindows.ConnectionResult(initialData.lr);
                    }
                    else // Runs if wanting to register
                    {
                        ConnectWindows.ConnectionResult(initialData.lr);
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
                    PlayersManager.everyPlayersPosition = JsonSerializer.Deserialize(packet.data, EveryPlayersPositionContext.Default.EveryPlayersPosition);
                    NodeManager.playersManager.CallDeferred(nameof(NodeManager.playersManager.ProcessOtherPlayerPosition));
                    break;
                case 4:
                    EveryPlayersName everyPlayersName = JsonSerializer.Deserialize(packet.data, EveryPlayersNameContext.Default.EveryPlayersName);
                    break;
            }
        }
        catch
        {
            //GD.Print("Packet error");
        }
    }
    static byte[] EncodeMessage(byte commandType, string message)
    {
        if (Encryption.encryption)
        {
            return Encryption.Encrypt($"#{commandType}#${message}$");
        }
        else
        {
            return Encoding.ASCII.GetBytes($"#{commandType}#${message}$");
        }
    }
}

