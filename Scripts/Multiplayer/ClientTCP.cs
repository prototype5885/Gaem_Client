using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public partial class ClientTCP : Node
{
    TcpClient tcpClient; // TCP client

    MeshInstance3D OnlineObject;
    Label StatusLabel;

    //float[] ServerPositions = new float[16];

    public bool isConnected = false;

    //public Player localPlayer = new Player();
    PlayersManager playersManager;

    DataProcessing dataProcessing = new DataProcessing(); // Object that deals with managing players and fixing received packets



    public override void _Ready()
    {
        // init
        OnlineObject = GetNode<MeshInstance3D>("/root/Map/OnlineObject");
        StatusLabel = GetParent().GetChild<Label>(2);
        playersManager = GetNode<PlayersManager>("/root/Map/PlayersManager");
        // end
        StatusLabel.Text = "Not connected to server";
    }
    public void Connect(string ip, int port)
    {
        try
        {
            tcpClient = new TcpClient(ip, port);
            GD.Print("Connected to TCP server successfully");
            StatusLabel.Text = "Connected, authentication in progress";
            isConnected = true;

        }
        catch (Exception ex)
        {
            GD.Print($"Failed to connect, exception: {ex}");
            StatusLabel.Text = "Failed to connect";
        }
    }

    public int Authentication(bool LoginOrRegister, string username, string password)
    {
        GD.Print("Authentication started");
        PasswordHasher passwordHasher = new PasswordHasher();
        string hashedPassword = passwordHasher.HashPassword(password + "secretxd");

        // Sends username / pass to server
        LoginData loginData = new LoginData
        {
            lr = LoginOrRegister,
            un = username,
            pw = hashedPassword
        };

        NetworkStream authenticationStream = tcpClient.GetStream();
        int receivedCode = 0;
        try
        {
            string jsonData = JsonSerializer.Serialize(loginData, LoginDataContext.Default.LoginData);
            byte[] messageByte = Encoding.ASCII.GetBytes($"#{jsonData.Length}#" + jsonData); // Adds the length to the beginning of message
            authenticationStream.Write(messageByte, 0, messageByte.Length);
            //authenticationStream.Close();

            // Waits for reply
            byte[] receivedBytes = new byte[1024];
            int bytesRead;

            bytesRead = authenticationStream.Read(receivedBytes, 0, receivedBytes.Length);
            string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);

            receivedCode = int.Parse(receivedData);
        }
        catch // Runs if there is no connection to the server
        {
            return -1;
        }

        if (LoginOrRegister == true) // Runs if wanting to login
        {
            if (receivedCode == 1) // Login successful
            {
                playersManager.SpawnPlayer();
                AuthenticationSuccessful();
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
                AuthenticationSuccessful();
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
    void AuthenticationSuccessful()
    {
        GD.Print("Authentication successful, waiting for server to send initial data");
        StatusLabel.Text = "Authentication successful, waiting for server to send initial data";

        NetworkStream receivingInitStream = tcpClient.GetStream();

        try
        {
            byte[] receivedBytes = new byte[1024];
            int bytesRead = receivingInitStream.Read(receivedBytes, 0, receivedBytes.Length);
            string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);
            receivingInitStream.Flush();

            InitialData initialData = JsonSerializer.Deserialize(receivedData, InitialDataContext.Default.InitialData);

            playersManager.PreSpawnPuppets(initialData.i, initialData.mp); // Sets the max amount of players the server can have and sets the index so the puppet of the local player wont be visible

            Task.Run(() => ReceiveDataTCP());
            Task.Run(() => SendDataTCP());

            GD.Print("Connected");
            StatusLabel.Text = "Connected";
        }
        catch (Exception ex)
        {
            GD.Print(ex);
            return;
        }

    }

    async Task ReceiveDataTCP()
    {
        NetworkStream receivingStream = tcpClient.GetStream();
        while (true)
        {
            try
            {
                byte[] receivedBytes = new byte[1024];
                int bytesRead;

                bytesRead = await receivingStream.ReadAsync(receivedBytes, 0, receivedBytes.Length);
                string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);

                playersManager.players = JsonSerializer.Deserialize(receivedData, PlayersContext.Default.Players);
                playersManager.CallDeferred(nameof(playersManager.ProcessOtherPlayerPosition));

                //await receivingStream.FlushAsync();
                //client.Close();
            }
            catch
            {
                GD.Print("Error receiving TCP packet");
                //CallDeferred(nameof(LostConnection), ex.ToString());
                //break;
            }
        }
    }
    async Task SendDataTCP()
    {
        NetworkStream sendingStream = tcpClient.GetStream();
        while (true)
        {
            try
            {
                StreamReader reader = new StreamReader(sendingStream);

                string jsonData = JsonSerializer.Serialize(playersManager.localPlayer, PlayerContext.Default.Player);
                byte[] messageByte = Encoding.ASCII.GetBytes($"#{jsonData.Length}#" + jsonData); // Adds the length to the beginning of message
                await sendingStream.WriteAsync(messageByte, 0, messageByte.Length);
                //await sendingStream.FlushAsync();
            }
            catch
            {
                GD.Print("Error sending TCP packet");
                //CallDeferred(nameof(LostConnection), ex.ToString());
                //break;
            }
            Thread.Sleep(50);
        }
    }
    void LostConnection(string ex)
    {
        GD.Print(ex);
        StatusLabel.Text = "Disconnected";
        isConnected = false;
        SetProcessInput(true);
    }
    void setObjectPosition(float newPosition)
    {
        OnlineObject.Position = OnlineObject.Position with { X = newPosition / 20 };
    }
}