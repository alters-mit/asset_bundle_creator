using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// URDF data for a continuous joint.
    /// </summary>
    public class UrdfContinuousJoint : UrdfJoint
    {
        /// <summary>
        /// The joint axis.
        /// </summary>
        public Vector3 axis;


        public UrdfContinuousJoint(XElement element, CoordinateSpace coordinateSpace) : base(element, coordinateSpace)
        {
            XElement axisElement = element.Element("axis");
            XAttribute xyzAttribute = axisElement.Attribute("xyz");
            if (xyzAttribute != null)
            {
                float[] arr = xyzAttribute.Value.ToArray();
                axis = new Vector3(arr[0], arr[1], arr[2]);
            }
            else
            {
                Debug.LogWarning("Couldn't get axis: " + element.Value);
                axis = default;
            }
        }
    }
}