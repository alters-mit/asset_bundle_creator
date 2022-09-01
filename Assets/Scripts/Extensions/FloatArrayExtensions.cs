using UnityEngine;


/// <summary>
/// Extensions for float arrays.
/// </summary>
public static class FloatArrayExtensions
{
    /// <summary>
    /// Convert an array to a Vector3.
    /// </summary>
    /// <param name="values">The values.</param>
    public static Vector3 ToVector3(this float[] values)
    {
        return new Vector3(values[0], values[1], values[2]);
    }
}