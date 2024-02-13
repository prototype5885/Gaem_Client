using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


public class PacketProcessing
{
    private Socket socket;

    private List<string> sentPackets = new List<string>();

    public PacketProcessing(Socket socket)
    {
        this.socket = socket;
    }

    public async Task SendUnreliable(byte commandType, string message, EndPoint address)
    {
        try
        {
            byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#{message}");

            if (address == null)
            {
                await socket.SendAsync(messageByte, SocketFlags.None);
            }
            else
            {
                await socket.SendToAsync(messageByte, SocketFlags.None, address);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending unreliable message type {commandType}. Exception: {ex.Message}");
        }
    }
    public async Task SendReliable(byte commandType, string message, EndPoint address)
    {

        byte[] messageByte = Encoding.ASCII.GetBytes($"#{commandType}#{message}");
        sentPackets.Add(message);
        Console.WriteLine($"Packet {message} has been added to the packet list waiting for reply...");

        int attempts = 0;
        while (sentPackets.Contains(message))
        {
            if (attempts > 10)
            {
                Console.WriteLine("Packet delivery failed");
                break;
            }
            if (address == null)
            {
                await socket.SendAsync(messageByte, SocketFlags.None);
            }
            else
            {
                await socket.SendToAsync(messageByte, SocketFlags.None, address);
            }

            Thread.Sleep(100);
            attempts++;
            Console.WriteLine($"Sent packet {message}, attempts: {attempts}");
        }
    }
    public void AcknowledgeReceived(string message)
    {
        sentPackets.Remove(message);
    }
    public Packet BreakUpPacket(byte[] receivedBytes, int byteLength)
    {
        string rawPacketString = Encoding.ASCII.GetString(receivedBytes, 0, byteLength);

        Packet packet = new Packet();


        string packetLengthPattern = @"#(.*)#";
        Match match = Regex.Match(rawPacketString, packetLengthPattern);
        if (match.Success)
        {
            // Extract the value between the '#' characters
            string extractedValue = match.Groups[1].Value;
            int.TryParse(extractedValue, out int typeOfPacket);

            packet.type = typeOfPacket;

            int firstHashIndex = rawPacketString.IndexOf('#');
            int secondHashIndex = rawPacketString.IndexOf('#', firstHashIndex + 1) + 1;

            int lengthOfPackage = rawPacketString.Length - secondHashIndex;

            packet.data = rawPacketString.Substring(secondHashIndex, lengthOfPackage);

            return packet;
        }
        else
        {
            return null;
        }
    }
}

