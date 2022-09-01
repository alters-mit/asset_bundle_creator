using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// URDF data for a revolute joint.
    /// </summary>
    public class UrdfRevoluteJoint : UrdfContinuousJoint
    {
        /// <summary>
        /// The lower limit of rotation.
        /// </summary>
        public float limitLower;
        /// <summary>
        /// The upper limit of rotation.
        /// </summary>
        public float limitUpper;
        /// <summary>
        /// The force limit.
        /// </summary>
        public float forceLimit = 0;


        public UrdfRevoluteJoint(XElement element, CoordinateSpace coordinateSpace) : base(element, coordinateSpace)
        {
            XElement limitElement = element.Element("limit");
            limitLower = float.Parse(limitElement.Attribute("lower").Value) * Mathf.Rad2Deg;
            limitUpper = float.Parse(limitElement.Attribute("upper").Value) * Mathf.Rad2Deg;
            XAttribute effort = limitElement.Attribute("effort");
            if (effort != null)
            {
                forceLimit = float.Parse(effort.Value);
            }
        }
    }
}