using Godot;
using System;
using System.Timers;

public partial class Hud : MarginContainer
{
    public static Label statusLabel;
    public static Panel pingingIndicator;

    public Label latencyLabel;

    private System.Timers.Timer pingingIndicatorTimer = new System.Timers.Timer(250);
    public override void _Ready()
    {
        statusLabel = GetNode<Label>("Debug/StatusLabel");
        pingingIndicator = GetNode<Panel>("Debug/PingingIndicator");
        latencyLabel = GetNode<Label>("Debug/LatencyLabel");

        //pingingIndicator.Visible = false;

        pingingIndicatorTimer.Elapsed += PingingIndicatorExpired;
        pingingIndicatorTimer.AutoReset = false;

        pingingIndicatorTimer.Start();

        SetConnectionStatusText(0);
    }
    public void PingReceived()
    {
        CallDeferred(nameof(ChangePingingIndicatorVisibility), true);
        pingingIndicatorTimer.Start();
    }

    private void PingingIndicatorExpired(object sender, ElapsedEventArgs e)
    {
        CallDeferred(nameof(ChangePingingIndicatorVisibility), false);
    }

    private static void ChangePingingIndicatorVisibility(bool visible)
    {
        //if (visible == false)
        //{
        //    pingingIndicator.Modulate = new Color(255, 0, 0);
        //}
        //else
        //{
        //    pingingIndicator.Modulate = new Color(0, 255, 0);
        //}
        pingingIndicator.Visible = visible;
    }
    public void UpdateLatencyOnHud(int latency)
    {
        latencyLabel.Text = latency.ToString();
    }
    public void SetConnectionStatusText(byte connectionStatus)
    {
        switch (connectionStatus)
        {
            case 0:
                statusLabel.Text = "Not connected";
                break;
            case 1:
                statusLabel.Text = "Connected";
                break;
            case 2:
                statusLabel.Text = "Timing out";
                break;
            case 3:
                statusLabel.Text = "Lost connection";
                break;
        }
    }
}