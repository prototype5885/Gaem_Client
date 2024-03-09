using Godot;
using System.Text;
using System.Threading.Tasks;
public partial class Chat : Panel
{
    private static readonly PackedScene chatMessageScene = GD.Load<PackedScene>("res://Components/chat/chatMessage.tscn");
    private static bool SpamCooldownEnded = true;

    private static LineEdit inputChat;
    private static Panel inputPanel;
    private static MarginContainer messagesMargin;
    private static Timer hideChatTimer;
    private static VBoxContainer messages;
    private static Timer spamTimer;

    public override void _Ready()
    {
        // init
        inputChat = GetNode<LineEdit>("%InputChat");
        inputPanel = GetNode<Panel>("%InputPanel");
        messagesMargin = GetChild<MarginContainer>(0);
        hideChatTimer = GetChild<Timer>(1);
        messages = messagesMargin.GetChild<VBoxContainer>(0);
        spamTimer = GetNode<Timer>("%SpamTimer");
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
                    return;
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
    //public override void _UnhandledInput(InputEvent @event)
    //{
    //    if (@event is InputEventMouseButton)
    //    {
    //        CleanMessage();
    //    }
    //}
    private void StartTyping()
    {
        NodeManager.gui.PlayerControlsEnabled(false); // disable controls for player
        messagesMargin.Visible = true;
        inputPanel.Visible = true;

        inputChat.PlaceholderText = "";
        hideChatTimer.Stop();

        inputChat.Editable = true;
        inputChat.GrabFocus();

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void PrepareMessage(string message)
    {
        if (message != "")
        {
            Task.Run(() => PacketProcessor.SendTcp(2, message));

            SpamCooldownEnded = false;
            spamTimer.Start();
        }
        CleanMessage();
    }

    private void CleanMessage()
    {
        NodeManager.gui.PlayerControlsEnabled(true); // enable controls for player
        inputChat.PlaceholderText = "Press enter to Chat";
        hideChatTimer.Start();
        inputChat.Text = "";

        // enables input after message is sent
        Input.MouseMode = Input.MouseModeEnum.Captured;

        inputPanel.Visible = false;
        inputChat.Editable = false;
    }
    
    public static void AddChatMessageToChatWindow(ChatMessage chatMessage)
    {
        HBoxContainer chatMessageLine = chatMessageScene.Instantiate<HBoxContainer>(); // instantiates the component that displays message
    
        chatMessageLine.GetChild<Label>(0).Text = chatMessage.i.ToString();
        chatMessageLine.GetChild<Label>(2).Text = chatMessage.m;
    
        messages.AddChild(chatMessageLine, true);
    
        if (messages.GetChildCount() > 10)
        {
            messages.GetChild(0).QueueFree();
        }
    
        messagesMargin.Visible = true;
        hideChatTimer.Start();
    }

    private void _on_spam_timer_timeout()
    {
        SpamCooldownEnded = true;
    }

    private void _on_input_chat_gui_input(InputEvent @event)
    {
        //if (@event is InputEventMouseButton)
        //{
        //    if (Input.IsActionJustPressed("left_click"))
        //    {
        //        enter_message();
        //    }
        //}
    }

    private void _on_hide_chat_timer_timeout()
    {
        messagesMargin.Visible = false;
        inputPanel.Visible = false;
    }
}
