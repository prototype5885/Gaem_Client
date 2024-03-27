using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using ProToTypeLounge.Scripts.Multiplayer;

public static class PacketProcessor
{
    public static void
        ProcessReceivedBytes(byte[] buffer, int byteLength) // processes bytes received from the buffer
    {
        // trims the buffer down
        byte[] receivedBytes = new byte[byteLength];
        Array.Copy(buffer, receivedBytes, byteLength);

        // decrypts or decodes the message
        string receivedBytesInString = Decode(receivedBytes);

        // separate if multiple packets were read as one
        List<Packet> packets = SeparatePackets(receivedBytesInString);

        // process each packet
        foreach (Packet packet in packets) ProcessDataSentByServer(packet);
    }

    private static string Decode(byte[] receivedBytes)
    {
        string receivedBytesInString;
        if (Encryption.encryptionEnabled) // runs if encryption is enabled
            receivedBytesInString = Encryption.Decrypt(receivedBytes);
        else // runs if encryption is disabled
            receivedBytesInString = Encoding.UTF8.GetString(receivedBytes);
        return receivedBytesInString;
    }

    private static List<Packet> SeparatePackets(string receivedBytesInString)
    {
        string packetTypePattern = @"#(.*)#"; // pattern to read the packet type
        string packetDataPattern = @"\$(.*?)\$"; // pattern to read the packet data

        MatchCollection packetTypeMatches = Regex.Matches(receivedBytesInString, packetTypePattern);
        MatchCollection packetDataMatches = Regex.Matches(receivedBytesInString, packetDataPattern);

        List<Packet> packets = new List<Packet>();
        for (int i = 0; i < packetTypeMatches.Count; i++) // saves all the packets found in the buffer
        {
            int.TryParse(packetTypeMatches[i].Groups[1].Value, out int typeOfPacket);

            Packet packet = new Packet();
            packet.type = typeOfPacket;
            packet.data = packetDataMatches[i].Groups[1].Value;

            packets.Add(packet);
        }

        return packets;
    }

    private static void ProcessDataSentByServer(Packet packet)
    {
        switch (packet.type)
        {
            // Type 0 means server is pinging the client
            case 0:
                GD.Print("Ping received from server");
                Task.Run(() => SendPacket.SendTcp(0, ""));
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
                List<PlayerPosition> everyPlayersPosition =
                    JsonSerializer.Deserialize<List<PlayerPosition>>(packet.data);
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
}