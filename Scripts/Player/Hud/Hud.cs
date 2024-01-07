using Godot;
using System;

public partial class Hud : Control
{
    Label info;
    Label MultiplayerInfo;
    // Stats stats;
    //MultiplayerManager MultiplayerManager;

    public override void _Ready()
    {
        // init
        info = GetNode<Label>("Info");
        MultiplayerInfo = GetNode<Label>("InfoMultiplayer");
        // stats = GetNode<Stats>("%Stats");
        Timer time = GetNode<Timer>("/root/Map/TimeLoop");
        //MultiplayerManager = GetNode<MultiplayerManager>("/root/Map/MultiplayerManager");
        // end

        time.Connect("timeout", new Callable(this, nameof(update_info)));

        //MultiplayerManager.Connect("PlayerConnected", new Callable(this, nameof(connected)));

        // update_info();
    }
    public void update_info()
    {
        // info.Text = "Money: " + stats.money.ToString() + "\n" + "Wages: " + stats.wage.ToString();
    }
    //public void connected()
    //{
    //    MultiplayerInfo.Text = Multiplayer.GetUniqueId().ToString();
    //}
}