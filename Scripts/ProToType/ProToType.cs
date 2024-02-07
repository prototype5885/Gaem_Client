using System;
using System.ComponentModel.DataAnnotations;
using Godot;

namespace ProToType;
public class ProToMath
{
    // Linear interpolation function
    public static Vector3 ProToLerp(Vector3 start, Vector3 end, float t)
    {
        t = Math.Clamp(t, 0.0f, 1.0f);

        float x = start.X + (end.X - start.X) * t;
        float y = start.Y + (end.Y - start.Y) * t;
        float z = start.Z + (end.Z - start.Z) * t;

        return new Vector3(x, y, z);
    }
    public static float ProToCrossProduct(Vector3 v1, Vector3 v2)
    {
        return v1.X * v2.X + v1.X * v2.X + v1.Z * v2.Z;
    }
    public static T ProToClamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            return min;
        }
        else if (value.CompareTo(max) > 0)
        {
            return max;
        }
        else
        {
            return value;
        }
    }
    public static System.Numerics.Vector3 GodotVectorToSystemVector(Godot.Vector3 godotVector)
    {
        return new System.Numerics.Vector3(godotVector.X, godotVector.Y, godotVector.Z);
    }
    public static Godot.Vector3 SystemVectorToGodotVector(System.Numerics.Vector3 systemVector)
    {
        return new Godot.Vector3(systemVector.X, systemVector.Y, systemVector.Z);
    }
}
