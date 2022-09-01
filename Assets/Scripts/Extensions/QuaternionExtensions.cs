using UnityEngine;


/// <summary>
/// Extensions for quaternions.
/// </summary>
public static class QuaternionExtensions
{
    /// <summary>
    /// Convert a quaternion by a coordinate space.
    /// </summary>
    /// <param name="quaternion">(this)</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    public static Quaternion InCoordinateSpace(this Quaternion quaternion, CoordinateSpace coordinateSpace)
    {
        if (coordinateSpace == CoordinateSpace.unity)
        {
            return quaternion;
        }
        // Source: https://github.com/Unity-Technologies/URDF-Importer/blob/90f353e4352aae4df52fa2c05e49b804631d2a63/com.unity.robotics.urdf-importer/Runtime/Extensions/BuiltInExtensions.cs#L123-L126
        else if (coordinateSpace == CoordinateSpace.ros)
        {
            return new Quaternion(quaternion.y, -quaternion.z, -quaternion.x, quaternion.w);
        }
        else if (coordinateSpace == CoordinateSpace.partnet_mobility)
        {
            return Quaternion.identity;
        }
        else
        {
            throw new System.Exception(coordinateSpace.ToString());
        }
    }
}