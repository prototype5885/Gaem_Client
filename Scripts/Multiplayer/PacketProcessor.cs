using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public static class PacketProcessor
{
    public static async Task SendTcp(byte commandType, string message)
    {
        byte[] messageBytes = EncodeMessage(commandType, message);
        //CalculateLatency.sentBytesPerSecond += messageBytes.Length;
        await Client.tcpStream.WriteAsync(messageBytes);
    }
    public static async Task SendUdp(byte commandType, string message)
    {
        byte[] messageBytes = EncodeMessage(commandType, message);
        //CalculateLatency.sentBytesPerSecond += messageBytes.Length;
        await Client.clientUdpSocket.SendAsync(messageBytes, SocketFlags.None);
    }
    public static async Task ReceiveTcpData()
    {
        try
        {
            CancellationToken cancellationToken = Client.cancellationTokenSource.Token;

            byte[] buffer = new byte[4096];
            while (!cancellationToken.IsCancellationRequested)
            {
                
                int bytesRead = await Client.tcpStream.ReadAsync(new ArraySegment<byte>(buffer), cancellationToken);
                GD.Print("Bytes read:" + bytesRead);
                //CalculateLatency.receivedBytesPerSecond += bytesRead;
                Packet[] packets = ProcessBuffer(buffer, bytesRead);
                ProcessPackets(packets);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Receiving task was cancelled");
        }
        // catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionReset)
        // {
        //     // Handle sudden client disconnect (ConnectionReset)
        //     
        //     //PlayersManager.DisconnectClient(connectedClient);
        // }
        catch (Exception ex)
        {
            Console.WriteLine($"Player disconnected abruptly");
            Console.WriteLine(ex);
        }
    }
    public static async Task ReceiveUdpData()
    {
        try
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int bytesRead = await Client.clientUdpSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
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
    private static Packet[] ProcessBuffer(byte[] buffer, int byteLength) // processes bytes received from the buffer
    {
        string receivedBytesInString; // creates this empty string for to be used later

        if (Encryption.encryption) // runs if encryption is enabled
        {
            byte[] receivedBytes = new byte[byteLength];
            Array.Copy(buffer, receivedBytes, byteLength);
            receivedBytesInString = Encryption.Decrypt(receivedBytes);

        }
        else // runs if encryption is disabled
        {
            receivedBytesInString = Encoding.UTF8.GetString(buffer, 0, byteLength);
        }
        GD.Print(receivedBytesInString);

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
    private static void ProcessPackets(Packet[] packets)
    {
        foreach (Packet packet in packets)
        {
            ProcessDataSentByServer(packet);
        }
    }
    private static void ProcessDataSentByServer(Packet packet)
    {
        try
        {
            switch (packet.type)
            {
                // Type 0 means server is pinging the client
                case 0:
                    GD.Print("Ping received from server");
                    Task.Run(() => SendTcp(0, ""));
                    break;
                // Type 1 means server is responding to login/registering with initial data
                case 1: 
                    InitialData initialData = JsonSerializer.Deserialize<InitialData>(packet.data);
                    GD.Print($"InitialData received from server, data: {initialData}");
                    Client.Authentication(initialData);
                    break;
                // Type 2 means server is sending chat message from a player
                case 2:
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(packet.data);
                    GD.Print($"ChatMessage received from server, message: {chatMessage.m}");
                    Chat.AddChatMessageToChatWindow(chatMessage);
                    break;
                // Type 3 means server is sending position of other players
                case 3:
                    PlayerPosition[] everyPlayersPosition = JsonSerializer.Deserialize<PlayerPosition[]>(packet.data);
                    // GD.Print($"PlayerPosition[] received from server, data: {everyPlayersPosition[0]}");
                    PlayersManager.UpdateOtherPlayersPosition(everyPlayersPosition);
                    break;
                // Type 4 means server is sending player data such as name
                case 4:
                    PlayerData[] playerDataArray = JsonSerializer.Deserialize<PlayerData[]>(packet.data);
                    GD.Print($"PlayerData[] received from server, array length: {playerDataArray.Length}");
                    Client.PlayerDataArrived(playerDataArray);
                    break;
            }
        }
        catch
        {
            //GD.Print("Packet error");
        }
    }
    private static byte[] EncodeMessage(byte commandType, string message)
    {
        if (Encryption.encryption)
        {
            return Encryption.Encrypt($"#{commandType}#${message}$");
        }
        else
        {
            return Encoding.UTF8.GetBytes($"#{commandType}#${message}$");
        }
    }
}