using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProToTypeLounge.Scripts.Multiplayer;

public static class SendPacket
{
    public static async Task SendTcp(byte commandType, string message)
    {
        try
        {
            byte[] messageBytes = EncodeMessage(commandType, message);
            //CalculateLatency.sentBytesPerSecond += messageBytes.Length;
            await Client.tcpStream.WriteAsync(messageBytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static async Task SendUdp(byte commandType, string message)
    {
        try
        {
            byte[] messageBytes = EncodeMessage(commandType, message);
            //CalculateLatency.sentBytesPerSecond += messageBytes.Length;
            await Client.clientUdpSocket.SendAsync(messageBytes, SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static byte[] EncodeMessage(byte commandType, string message)
    {
        if (Encryption.encryptionEnabled)
            return Encryption.Encrypt($"#{commandType}#${message}$");
        return Encoding.UTF8.GetBytes($"#{commandType}#${message}$");
    }
}