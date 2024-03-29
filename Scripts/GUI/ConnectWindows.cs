using Godot;
using System;

public partial class ConnectWindows : Control
{
    private static string ip = "";
    private static int port = 0;

    public static bool wantingToRegister = false; // this is needed so when server sends back response about authentication, the authenticator will know

    private static Godot.Collections.Array<Godot.Node> children;

    //static GUI gui;

    private static Label connectionErrorLabel;

    private static Control addressWindow;
    private static Control loginWindow;
    private static Control registrationWindow;

    // login
    private static LineEdit loginUsernameInput;
    private static LineEdit loginPasswordInput;

    // registration
    private static LineEdit registrationUsernameInput;
    private static LineEdit firstPasswordInput;
    private static LineEdit secondPasswordInput;

    // address window
    private static LineEdit inputIP;
    private static LineEdit inputPort;

    public override void _Ready()
    {
        children = GetChildren();
        NodeManager.gui = GetParent<GUI>();

        // main windows init
        addressWindow = GetChild<Control>(0);
        loginWindow = GetChild<Control>(1);
        registrationWindow = GetChild<Control>(2);
        connectionErrorLabel = GetChild<Label>(3);

        // login init
        Panel loginPanel = loginWindow.GetChild<Panel>(0);
        loginUsernameInput = loginPanel.GetChild<LineEdit>(0);
        loginPasswordInput = loginPanel.GetChild<LineEdit>(1);

        loginPanel.GetChild<Button>(2).Pressed += () => Login(); // connects to server through login
        loginPanel.GetChild<Button>(3).Pressed += () => ChangeWindow(2); // opens registration window
        loginPanel.GetChild<Button>(4).Pressed += () => ChangeWindow(0); // opens address window

        // registration init
        Panel registrationPanel = registrationWindow.GetChild<Panel>(0);
        registrationUsernameInput = registrationPanel.GetChild<LineEdit>(0);
        firstPasswordInput = registrationPanel.GetChild<LineEdit>(1);
        secondPasswordInput = registrationPanel.GetChild<LineEdit>(2);

        registrationPanel.GetChild<Button>(3).Pressed += () => Register(); // connects to server through registration
        registrationPanel.GetChild<Button>(4).Pressed += () => ChangeWindow(1); // opens login window

        // address window init
        Panel addressPanel = addressWindow.GetChild<Panel>(0);
        inputIP = addressPanel.GetChild<LineEdit>(0);
        inputPort = addressPanel.GetChild<LineEdit>(1);

        addressPanel.GetChild<Button>(2).Pressed += () => ChangeServerAddress(); // changes server address

        // make login window visible on start
        CloseAllConnectWindows();

        // reads the server address, which in turns opens the login window on start
        ChangeServerAddress();
    }

    private static void Login()
    {
        string username = loginUsernameInput.Text;
        string password = loginPasswordInput.Text;

        Client.Connect(ip, port, username, password);
    }

    private static void Register()
    {
        wantingToRegister = true;

        string username = registrationUsernameInput.Text;
        string password = firstPasswordInput.Text;
        string secondPassword = secondPasswordInput.Text;

        if (password != secondPassword)
        {
            connectionErrorLabel.Visible = true;
            connectionErrorLabel.Text = "Passwords don't match";
            return;
        }
        if (username.Length < 2 || username.Length > 16)
        {
            connectionErrorLabel.Visible = true;
            connectionErrorLabel.Text = "Username is either too short or too long, min 2 characters, max 16";
            return;
        }
        if (password.Length < 2)
        {
            connectionErrorLabel.Visible = true;
            connectionErrorLabel.Text = "Password is too short, min 2 characters are required";
            return;
        }

        Client.Connect(ip, port, username, password);
    }
    public static bool ConnectionResult(byte receivedCode)
    {
        connectionErrorLabel.Visible = true;
        switch (receivedCode)
        {
            case 0:
                connectionErrorLabel.Text = "Failed to connect to the server.";
                return false;
            case 1: // successful login or registration
                CloseAllConnectWindows();
                NodeManager.gui.PlayerControlsEnabled(true);
                return true;
            case 2:
                connectionErrorLabel.Text = "Wrong username or password."; // in login
                return false;
            case 3:
                connectionErrorLabel.Text = "No user was found with this name"; // in login
                return false;
            case 4:
                connectionErrorLabel.Text = "This user is already logged in"; // in login
                return false;
            case 5:
                connectionErrorLabel.Text = "Username is either too short or too long, min 2 characters, max 16   "; // in registration
                return false;
            case 6:
                connectionErrorLabel.Text = "Username is already taken."; // in registration
                return false;
            case 7:
                connectionErrorLabel.Text = "Server is full.";
                return false;
        }
        return false;
    }

    private static void ChangeWindow(byte window)
    {
        CloseAllConnectWindows();
        switch (window)
        {
            case 0:
                addressWindow.Visible = true;
                break;
            case 1:
                loginWindow.Visible = true;
                break;
            case 2:
                registrationWindow.Visible = true;
                break;
        }
    }

    private static void ChangeServerAddress()
    {
        if (!int.TryParse(inputPort.Text, out int parsedPort))
        {
            connectionErrorLabel.Visible = true;
            connectionErrorLabel.Text = "Wrong port format";
            return;
        }
        ip = inputIP.Text;
        port = parsedPort;

        ChangeWindow(1);
    }

    private static void CloseAllConnectWindows()
    {
        foreach (Control window in children)
        {
            window.Visible = false;
        }
    }
}
