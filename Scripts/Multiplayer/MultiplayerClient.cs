using Godot;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class MultiplayerClient : Node
{
    TcpClient client;

    MeshInstance3D OnlineObject;
    Label StatusLabel;

    const string connectedStatus = "Connected";
    const string disconnectedStatus = "Disconnected";

    //float[] ServerPositions = new float[16];
    public string messageToSend = "";

    public bool isConnected = false;

    public override void _Ready()
    {
        // init
        OnlineObject = GetNode<MeshInstance3D>("/root/Map/OnlineObject");
        StatusLabel = GetChild<Label>(0);
        // end
        StatusLabel.Text = disconnectedStatus;
    }

    //public override void _Input(InputEvent @event)
    //{
    //    if (@event is InputEventKey)
    //    {
    //        if (Input.IsActionJustPressed("join"))
    //        {
    //            Connect("127.0.0.1", 1942);
    //        }
    //    }
    //}
    public void Connect(string ip, int port)
    {
        try
        {
            client = new TcpClient(ip, port);
            GD.Print("Connected");
            isConnected = true;
            StatusLabel.Text = connectedStatus;
            //Authentication(client);
            //StartDataTransmission();
            //SetProcessInput(false);
        }
        catch (Exception ex)
        {
            GD.Print($"Failed to connect, exception: {ex}");
        }
    }
    public bool Authentication(string username, string password)
    {
        // Sends username / pass to server
        NetworkStream authenticationStream = client.GetStream();


        Credentials credentials = new Credentials
        {
            un = username,
            pw = password
        };

        string jsonData = JsonConvert.SerializeObject(credentials);
        GD.Print(jsonData);

        byte[] data = Encoding.UTF8.GetBytes(jsonData);

        authenticationStream.Write(data, 0, data.Length);
        //authenticationStream.Close();

        // Waits for reply
        byte[] receivedBytes = new byte[1024];
        int bytesRead;

        bytesRead = authenticationStream.Read(receivedBytes, 0, receivedBytes.Length);

        string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);

        if (receivedData != "1")
        {
            GD.Print("Wrong username or password or some other error.");
            return false;
        }
        else
        {
            GD.Print("Username and password accepted.");
            StartDataTransmission();
            return true;
        }
    }
    void StartDataTransmission()
    {
        Task.Run(() => ReceivingDataFromServer());
        Task.Run(() => SendingDataToServer());
    }
    async Task ReceivingDataFromServer()
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
            catch (Exception ex)
            {
                CallDeferred(nameof(LostConnection), ex.ToString());
                break;
            }
        }
    }
    async Task SendingDataToServer()
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
                messageToSend = "lol";
            }
            catch (Exception ex)
            {
                CallDeferred(nameof(LostConnection), ex.ToString());
                break;
            }
            Thread.Sleep(50);
        }
    }
    void LostConnection(string ex)
    {
        GD.Print(ex);
        StatusLabel.Text = disconnectedStatus;
        isConnected = false;
        SetProcessInput(true);
    }
    void setObjectPosition(float newPosition)
    {
        OnlineObject.Position = OnlineObject.Position with { X = newPosition / 20 };
    }
}
public class Credentials
{
    public string un { get; set; }
    public string pw { get; set; }
}