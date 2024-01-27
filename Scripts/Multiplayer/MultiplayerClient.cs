using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
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

    public bool isConnected = false;

    public LocalPlayerPosition localPlayerPosition = new LocalPlayerPosition();
    PlayersManager playersManager;

    CharacterBody3D localPlayer;

    public override void _Ready()
    {
        // init
        OnlineObject = GetNode<MeshInstance3D>("/root/Map/OnlineObject");
        StatusLabel = GetChild<Label>(0);
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        //player = GetNode<>
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

        }
        catch (Exception ex)
        {
            GD.Print($"Failed to connect, exception: {ex}");
        }
    }
    public int Authentication(bool LoginOrRegister, string username, string password)
    {
        PasswordHasher passwordHasher = new PasswordHasher();
        string hashedPassword = passwordHasher.HashPassword(password + "secretxd");

        // Sends username / pass to server
        NetworkStream authenticationStream = client.GetStream();


        LoginData loginData = new LoginData
        {
            lr = LoginOrRegister,
            un = username,
            pw = hashedPassword
        };

        string jsonData = JsonSerializer.Serialize(loginData);

        byte[] data = Encoding.ASCII.GetBytes(jsonData);

        authenticationStream.Write(data, 0, data.Length);
        //authenticationStream.Close();

        // Waits for reply
        byte[] receivedBytes = new byte[1024];
        int bytesRead;

        bytesRead = authenticationStream.Read(receivedBytes, 0, receivedBytes.Length);

        string receivedData = Encoding.UTF8.GetString(receivedBytes, 0, bytesRead);

        int receivedCode = int.Parse(receivedData);


        if (LoginOrRegister == true) // Runs if wanting to login
        {
            if (receivedCode == 1) // Login successful
            {
                playersManager.SpawnPlayer();
                StartDataTransmission();
                return 1;
            }
            else // Possibly wrong username or password
            {
                return 0;
            }
        }
        else // Runs if wanting to register
        {
            if (receivedCode == 1) // Registration successful
            {
                playersManager.SpawnPlayer();
                StartDataTransmission();
                return 1;
            }
            else if (receivedCode == 2) // Username is longer than 16 characters
            {
                return 2;
            }
            else if (receivedCode == 3) // Username is already taken
            {
                return 3;
            }
            else // Unknown error
            {
                return 0;
            }
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

                //try
                //{
                //    EveryPlayerPosition everyPlayerPosition = JsonSerializer.Deserialize(receivedData, EveryPlayerPositionContext.Default.EveryPlayerPosition);
                //    playersManager.ProcessOtherPlayerPosition(everyPlayerPosition.epp);
                //}
                //catch (Exception ex)
                //{
                //    GD.Print(ex);
                //}

                await receivingStream.FlushAsync();
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

                string jsonData = JsonSerializer.Serialize(localPlayerPosition);
                byte[] messageByte = Encoding.ASCII.GetBytes($"{jsonData.Length}" + jsonData);
                await sendingStream.WriteAsync(messageByte, 0, messageByte.Length);
                await sendingStream.FlushAsync();
            }
            catch (Exception ex)
            {
                CallDeferred(nameof(LostConnection), ex.ToString());
                break;
            }
            Thread.Sleep(100);
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