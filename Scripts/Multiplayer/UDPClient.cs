using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


public partial class UDPClient : Node
{
    // udp stuff
    UdpClient udpServer = new UdpClient();

    PlayersManager playersManager;
    public Player localPlayer = new Player();

    public override void _Ready()
    {
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");


    }
    public bool ConnectToUDPServer()
    {
        try
        {
            //udpServer.Connect("127.0.0.1", 1943);


            //Task.Run(() => SendDataUDP());
            //Task.Run(() => ReceiveDataUDP());

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    async Task SendDataUDP()
    {
        while (true)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(localPlayer, PlayerContext.Default.Player);
                byte[] messageByte = Encoding.ASCII.GetBytes($"#{jsonData.Length}#" + jsonData);
                await udpServer.SendAsync(messageByte, messageByte.Length);
            }
            catch
            {
                GD.Print("error sending udp data");
            }
            Thread.Sleep(100);
        }
    }
    async Task ReceiveDataUDP()
    {
        IPEndPoint remoteEP;
        while (true)
        {
            try
            {
                UdpReceiveResult data = await udpServer.ReceiveAsync();
                string message = Encoding.UTF8.GetString(data.Buffer);
                remoteEP = data.RemoteEndPoint;
                GD.Print($"Message: {message}, from {remoteEP}");
            }
            catch
            {
                GD.Print("failed receiving udp data");
            }
        }
    }
}

