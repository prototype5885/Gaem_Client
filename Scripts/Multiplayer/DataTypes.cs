using System.Collections.Generic;
using System.Net;
using System.Numerics;

public class LoginData
{
    public bool lr { get; set; } // True if login, false if register
    public string un { get; set; } // Username
    public string pw { get; set; } // Password
}
public class PlayerPosition
{
    public float x { get; set; } // Player position X
    public float y { get; set; } // Player position Y
    public float z { get; set; } // Player position Z

    public float rx { get; set; } // Player head rotation X
    public float ry { get; set; } // Player body rotation Y


    public override string ToString()
    {
        return $"{(int)x}, {(int)y}, {(int)z}";
    }
}
public class EveryPlayersPosition
{
    public PlayerPosition[] positions { get; set; }
}
public class InitialData
{
    public int i { get; set; }
    public int mp { get; set; }
}
public class Packet
{
    public int type { get; set; }
    public string data { get; set; }
}
