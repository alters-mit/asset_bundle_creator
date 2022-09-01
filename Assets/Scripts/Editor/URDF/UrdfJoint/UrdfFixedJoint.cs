using System.Xml.Linq;


namespace URDF
{
    /// <summary>
    /// URDF data for a fixed joint.
    /// </summary>
    public class UrdfFixedJoint : UrdfJoint
    {
        public UrdfFixedJoint(XElement element, CoordinateSpace coordinateSpace) : base(element, coordinateSpace)
        {
        }
    }
}