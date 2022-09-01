using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Extensions for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts an XML string to an array of floats.
    /// </summary>
    /// <param name="str">(this)</param>
    public static float[] ToArray(this string str)
    {
        // Find all numbers. Source: https://stackoverflow.com/a/12643073
        MatchCollection matches = Regex.Matches(str, "[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)");
        List<float> values = new List<float>();
        foreach (Match m in matches)
        {
            values.Add(float.Parse(m.Value));
        }
        return values.ToArray();
    }


    /// <summary>
    /// Converts an XML string to a Vector3.
    /// </summary>
    /// <param name="str">(this)</param>
    public static Vector3 ToVector3(this string str)
    {
        float[] arr = str.ToArray();
        return new Vector3(arr[0], arr[1], arr[2]);
    }


    /// <summary>
    /// Converts an XML string to an (x, y, z) position in Unity coordinates.
    /// </summary>
    /// <param name="str">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 ToPosition(this string str, CoordinateSpace coordinateSpace)
    {
        return str.ToVector3().PositionInCoordinateSpace(coordinateSpace);
    }


    /// <summary>
    /// Converts an XML string to (x, y, z) Euler angles in Unity coordinates.
    /// </summary>
    /// <param name="str">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 ToEulerAngles(this string str, CoordinateSpace coordinateSpace)
    {
        return str.ToVector3().EulerAnglesInCoordinateSpace(coordinateSpace);
    }


    /// <summary>
    /// Converts an XML string to an (x, y, z) anchor position in Unity coordinates.
    /// </summary>
    /// <param name="str">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 ToAnchorPosition(this string str, CoordinateSpace coordinateSpace)
    {
        return str.ToVector3().AnchorInCoordinateSpace(coordinateSpace);
    }


    /// <summary>
    /// Converts an XML string to an (x, y, z) scale in Unity coordinates.
    /// </summary>
    /// <param name="str">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 ToScale(this string str, CoordinateSpace coordinateSpace)
    {
        return str.ToVector3().ScaleInCoordinateSpace(coordinateSpace);
    }


    /// <summary>
    /// Fix a Windows path.
    /// </summary>
    /// <param name="str">(this)</param>
    public static string FixWindowsPath(this string str)
    {
        return str.Replace("\\", "/");
    }


    /// <summary>
    /// Fix a string to make it URI-safe.
    /// </summary>
    /// <param name="str">(this)</param>
    public static string UriSafe(this string str)
    {
        return str.Replace("#", "_");
    }
}