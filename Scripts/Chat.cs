using Godot;
public partial class Chat : Panel
{
    PackedScene chatMessageScene;
    bool SpamCooldownEnded = true;
    Node clientDataManager;

    LineEdit inputChat;
    Panel inputPanel;
    MarginContainer messagesMargin;
    Timer hideChatTimer;
    VBoxContainer messages;
    Timer spamTimer;

    GUI GUI;

    ClientTCP clientTCP;

    public override void _Ready()
    {
        // init
        chatMessageScene = GD.Load<PackedScene>("res://Components/chat/chatMessage.tscn");
        inputChat = GetNode<LineEdit>("%InputChat");
        inputPanel = GetNode<Panel>("%InputPanel");
        messagesMargin = GetChild<MarginContainer>(0);
        hideChatTimer = GetChild<Timer>(1);
        messages = messagesMargin.GetChild<VBoxContainer>(0);
        spamTimer = GetNode<Timer>("%SpamTimer");
        GUI = GetParent<GUI>();
        clientTCP = GetNode<ClientTCP>("/root/Map/MultiplayerManager/ClientTCP");
        // end

        //inputChat.Visible = false;
        //inputPanel.Visible = false;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("chat"))
            {
                if (!inputChat.Editable)
                {
                    StartTyping();
                }
                // else if (canMessage)
                // {
                //     prepare_message(inputChat.Text);
                // }
            }
            if (Input.IsActionJustPressed("enter") && SpamCooldownEnded)
            {
                PrepareMessage(inputChat.Text);
            }
            else if (Input.IsActionJustPressed("ui_cancel"))
            {
                CleanMessage();
            }
        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            CleanMessage();
        }
    }
    private void StartTyping()
    {
        GUI.PlayerControlsEnabled(false); // disable controls for player
        messagesMargin.Visible = true;
        inputPanel.Visible = true;

        inputChat.PlaceholderText = "";
        hideChatTimer.Stop();

        inputChat.Editable = true;
        inputChat.GrabFocus();

        //inputChat.Text = "wtf"; // resets the chat message so it wont include letter t

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    void PrepareMessage(string message)
    {
        if (message != "")
        {
            //Rpc(nameof(SendMessage), message); // send to server
            //SendMessage(message);
            //mpClient.messageToSend = message;
            SpamCooldownEnded = false;
            spamTimer.Start();
        }
        CleanMessage();
    }
    void CleanMessage()
    {
        GUI.PlayerControlsEnabled(true); // enable controls for player
        inputChat.PlaceholderText = "Press enter to Chat";
        hideChatTimer.Start();
        inputChat.Text = "";

        // enables input after message is sent
        Input.MouseMode = Input.MouseModeEnum.Captured;

        inputPanel.Visible = false;
        inputChat.Editable = false;
    }

    //[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    //void SendMessage(string message)
    //{
    //    HBoxContainer chatMessage = chatMessageScene.Instantiate<HBoxContainer>();

    //    chatMessage.GetNode<Label>("Message").Text = message;
    //    chatMessage.GetNode<Label>("Name").Text = Multiplayer.GetRemoteSenderId().ToString();

    //    messages.AddChild(chatMessage, true);

    //    if (messages.GetChildCount() > 10)
    //    {
    //        messages.GetChild(0).QueueFree();
    //    }

    //    messagesMargin.Visible = true;
    //    hideChatTimer.Start();
    //}
    void _on_spam_timer_timeout()
    {
        SpamCooldownEnded = true;
    }
    void _on_input_chat_gui_input(InputEvent @event)
    {
        //if (@event is InputEventMouseButton)
        //{
        //    if (Input.IsActionJustPressed("left_click"))
        //    {
        //        enter_message();
        //    }
        //}
    }
    void _on_hide_chat_timer_timeout()
    {
        messagesMargin.Visible = false;
        inputPanel.Visible = false;
    }
}
