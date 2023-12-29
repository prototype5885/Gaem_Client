using Godot;
using System;

public partial class Hud : Control
{
    Label info;
    Stats stats;

    public override void _Ready()
    {
        // init
        info = GetNode<Label>("Info");
        stats = GetNode<Stats>("%Stats");
        Timer time = GetNode<Timer>("/root/Map/TimeLoop");
        // end

        time.Connect("timeout", new Callable(this, nameof(update_info)));

        update_info();
    }
    public void update_info()
    {
        info.Text = "Money: " + stats.money.ToString() + "\n" + "Wages: " + stats.wage.ToString();
    }
}