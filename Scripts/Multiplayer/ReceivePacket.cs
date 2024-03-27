using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace ProToTypeLounge.Scripts.Multiplayer;

public static class ReceivePacket
{
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
                PacketProcessor.ProcessReceivedBytes(buffer, bytesRead);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Receiving task was cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving TCP packet");
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
                int bytesRead =
                    await Client.clientUdpSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                //CalculateLatency.receivedBytesPerSecond += bytesRead;
                PacketProcessor.ProcessReceivedBytes(buffer, bytesRead);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving UDP packet");
            Console.WriteLine(ex);
        }
    }
}