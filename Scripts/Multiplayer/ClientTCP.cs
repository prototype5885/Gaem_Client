using Godot;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

public partial class ClientTCP : Node
{
    TcpClient tcpClient;
    MeshInstance3D OnlineObject;

    //float newPosition;

    public override void _Ready()
    {
        //SetProcess(false);
        OnlineObject = GetNode<MeshInstance3D>("/root/Map/OnlineObject");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("join"))
            {
                //ConnectToHost("127.0.0.1", 1942);
                Task.Run(() => ConnectToHost("127.0.0.1", 1942));
            }
        }
    }
    async Task ConnectToHost(string ipAddress, int port)
    {
        //string serverIpAddress = "127.0.0.1";
        //int serverPort = 1942;

        //tcpClient = new TcpClient();

        //tcpClient.Connect(serverIpAddress, serverPort);

        //Console.WriteLine("Connected to the server");

        // Replace "127.0.0.1" and 12345 with your server's IP address and port number

        tcpClient = new TcpClient();

        try
        {
            // Connect to the server
            tcpClient.Connect(ipAddress, port);

            GD.Print("Connected to the server");

            // Get the network stream for reading data
            NetworkStream networkStream = tcpClient.GetStream();

            // Receive messages continuously
            while (true)
            {

                byte[] buffer = new byte[256];
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);

                //byte[] trimmedBuffer = new byte[bytesRead];
                //for (int i = 0; i < bytesRead; i++)
                //{
                //    trimmedBuffer[i] = buffer[i];
                //}

                //GD.Print(trimmedBuffer[0].ToString());

                if (bytesRead > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                    float floatValue;
                    if (float.TryParse(receivedMessage, out floatValue))
                    {
                        // Conversion successful, use the converted float value
                        Console.WriteLine("Converted float value: " + floatValue);
                    }
                    else
                    {
                        // Conversion failed, handle the error
                        Console.WriteLine("Invalid input string");
                    }

                    //newPosition = floatValue;


                    GD.Print("Received message: " + receivedMessage);

                    CallDeferred(MethodName.setObjectPosition, floatValue);

                }
            }
        }
        catch (Exception ex)
        {
            GD.Print("Error: " + ex.Message);
        }
        finally
        {
            // Close the TCP client when done
            if (tcpClient != null && tcpClient.Connected)
            {
                tcpClient.Close();
            }
        }

    }
 
    void setObjectPosition(float position)
    {
        OnlineObject.Position = OnlineObject.Position with { X = position / 20 };
    }
    //public override void _Process(double delta)
    //{
    //    Vector3 localPos = OnlineObject.Position;
    //    localPos.X = Mathf.Lerp(localPos.X, newPosition / 10, (float)delta);
    //    OnlineObject.Position = localPos;
    //}
    //public override void _Process(double delta)
    //{
    //    // Check for incoming data
    //    tcpClient.Poll();
    //}
}