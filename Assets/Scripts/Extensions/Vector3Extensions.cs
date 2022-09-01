using UnityEngine;


/// <summary>
/// Extensions for Vector3.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Convert a position vector by a coordinate space.
    /// </summary>
    /// <param name="vector3">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 PositionInCoordinateSpace(this Vector3 vector3, CoordinateSpace coordinateSpace)
    {
        if (coordinateSpace == CoordinateSpace.unity)
        {
            return vector3;
        }
        // Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/Extensions/BuiltInExtensions.cs
        else if (coordinateSpace == CoordinateSpace.ros)
        {
            return new Vector3(-vector3.y, vector3.z, vector3.x);
        }
        else if (coordinateSpace == CoordinateSpace.partnet_mobility)
        {
            return Vector3.zero;
        }
        else
        {
            throw new System.Exception(coordinateSpace.ToString());
        }
    }


    /// <summary>
    /// Convert a joint anchor position vector by a coordinate space.
    /// </summary>
    /// <param name="vector3">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 AnchorInCoordinateSpace(this Vector3 vector3, CoordinateSpace coordinateSpace)
    {
        if (coordinateSpace == CoordinateSpace.unity)
        {
            return vector3;
        }
        else if (coordinateSpace == CoordinateSpace.ros)
        {
            return Vector3.zero;
        }
        else if (coordinateSpace == CoordinateSpace.partnet_mobility)
        {
            return vector3;
        }
        else
        {
            throw new System.Exception(coordinateSpace.ToString());
        }
    }


    /// <summary>
    /// Convert an Euler angles vector by a coordinate space.
    /// </summary>
    /// <param name="vector3">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 EulerAnglesInCoordinateSpace(this Vector3 vector3, CoordinateSpace coordinateSpace)
    {
        if (coordinateSpace == CoordinateSpace.unity)
        {
            return vector3;
        }
        else if (coordinateSpace == CoordinateSpace.partnet_mobility)
        {
            return Vector3.zero;
        }
        // Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/Extensions/BuiltInExtensions.cs
        else if (coordinateSpace == CoordinateSpace.ros)
        {
            return new Vector3(
                -vector3.y * Mathf.Rad2Deg,
                vector3.z * Mathf.Rad2Deg,
                -vector3.x * Mathf.Rad2Deg);
        }
        else
        {
            throw new System.Exception(coordinateSpace.ToString());
        }
    }


    /// <summary>
    /// Convert a scale vector by a coordinate space.
    /// </summary>
    /// <param name="vector3">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Vector3 ScaleInCoordinateSpace(this Vector3 vector3, CoordinateSpace coordinateSpace)
    {
        if (coordinateSpace == CoordinateSpace.unity)
        {
            return vector3;
        }
        else if (coordinateSpace == CoordinateSpace.partnet_mobility)
        {
            return vector3;
        }
        // Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/Extensions/BuiltInExtensions.cs
        else if (coordinateSpace == CoordinateSpace.ros)
        {
            return new Vector3(vector3.y, vector3.z, vector3.x);
        }
        else
        {
            throw new System.Exception(coordinateSpace.ToString());
        }
    }


    /// <summary>
    /// Returns a JSON string of this vector.
    /// </summary>
    /// <param name="vector">(this)</param>
    public static string ToJsonString(this Vector3 vector)
    {
        return "{\"x\": " + vector.x + ", \"y\": " + vector.y + ", \"z\": " + vector.z + "}";
    }
}