using Godot;


public partial class Chat : Panel
{
    PackedScene chatMessageScene;
    bool canMessage = true;
    Node clientDataManager;

    LineEdit inputChat;
    Panel inputPanel;
    MarginContainer messagesMargin;
    Timer hideChatTimer;
    VBoxContainer messages;
    Timer spamTimer;

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
        // end

        //inputChat.Visible = false;
        //inputPanel.Visible = false;
        //SetProcessUnhandledInput(false);
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            if (Input.IsActionJustPressed("ui_accept"))
            {
                if (!inputChat.Editable)
                {
                    start_typing();
                }
                else if (canMessage)
                {
                    prepare_message(inputChat.Text);
                }
            }
            else if (Input.IsActionJustPressed("ui_cancel"))
            {
                clean_message();
            }
        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            clean_message();
        }
    }
    void start_typing()
    {
        messagesMargin.Visible = true;
        inputPanel.Visible = true;
        inputChat.PlaceholderText = "";
        hideChatTimer.Stop();

        inputChat.Editable = true;
        inputChat.GrabFocus();

        // disables other input while typing
        // need stuff
        SetProcessUnhandledInput(true);
        //Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    void prepare_message(string message)
    {
        if (message != "")
        {
            Rpc("send_message", message);
            canMessage = false;
            spamTimer.Start();
        }
        clean_message();
    }
    void clean_message()
    {
        inputChat.PlaceholderText = "Press enter to Chat";
        hideChatTimer.Start();
        inputChat.Text = "";

        // enables input after message is sent
        SetProcessUnhandledInput(false);
        //Input.MouseMode = Input.MouseModeEnum.Visible;

        inputPanel.Visible = false;
        inputChat.Editable = false;
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    void send_message(string message)
    {
        //HBoxContainer chatMessage = chatMessageScene.Instantiate<HBoxContainer>();

        //chatMessage.GetNode<Label>("Message").Text = message;
        //chatMessage.GetNode<Label>("Name").Text = Multiplayer.GetRemoteSenderId().ToString();


        //messages.AddChild(chatMessage, true);

        //if (messages.GetChildCount() > 10)
        //{
        //    messages.GetChild(0).QueueFree();
        //}

        //messagesMargin.Visible = true;
        //hideChatTimer.Start();
    }
    void _on_spam_timer_timeout()
    {
        canMessage = true;
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
