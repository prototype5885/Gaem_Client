using Godot;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class ClientTCP : Node
{
    MeshInstance3D OnlineObject;

    //float[] ServerPositions = new float[16];
    public string messageToSend = "";

    public override void _Ready()
    {
        // init
        OnlineObject = GetNode<MeshInstance3D>("/root/Map/OnlineObject");
        // end
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("join"))
            {
                Connect();

            }
        }
    }
    void Connect()
    {
        TcpClient client = new TcpClient("127.0.0.1", 1942);
        Task.Run(() => ReceivingDataFromServer(client));
        Task.Run(() => SendingDataToServer(client));
        SetProcessInput(false);
    }
    async Task ReceivingDataFromServer(TcpClient client)
    {
        NetworkStream receivingStream = client.GetStream();
        while (true)
        {
            try
            {
                byte[] receivedBytes = new byte[1024];
                int bytesRead;

                bytesRead = await receivingStream.ReadAsync(receivedBytes, 0, receivedBytes.Length);

                string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);


                GD.Print($"Received from server: {receivedData}");

                receivingStream.Flush();
            }
            catch (IOException)
            {
                // Server disconnected
                break;
            }
        }
    }

    async Task SendingDataToServer(TcpClient client)
    {
        NetworkStream sendingStream = client.GetStream();
        while (true)
        {
            try
            {
                StreamReader reader = new StreamReader(sendingStream);

                byte[] messageByte = Encoding.ASCII.GetBytes(messageToSend);
                await sendingStream.WriteAsync(messageByte, 0, messageByte.Length);

                sendingStream.Flush();
                messageToSend = "";
            }
            catch (Exception ex)
            {
                GD.Print(ex);
            }
            Thread.Sleep(50);
        }
    }
    void setObjectPosition(float newPosition)
    {
        OnlineObject.Position = OnlineObject.Position with { X = newPosition / 20 };
    }
}