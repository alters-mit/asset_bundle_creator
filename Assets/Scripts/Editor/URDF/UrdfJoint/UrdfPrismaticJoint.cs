using System.Xml.Linq;
using TDW.Robotics;


namespace URDF
{
    /// <summary>
    /// URDF data for a prismatic joint.
    /// </summary>
    public class UrdfPrismaticJoint : UrdfRevoluteJoint
    {
        /// <summary>
        /// The expected drive axis.
        /// </summary>
        public DriveAxis driveAxis;


        public UrdfPrismaticJoint(XElement element, CoordinateSpace coordinateSpace) : base(element, coordinateSpace)
        {
            // Get the drive axis for a single-axis rotation.
            if (axis.x != 0)
            {
                driveAxis = DriveAxis.x;
            }
            else if (axis.y != 0)
            {
                driveAxis = DriveAxis.y;
            }
            else if (axis.z != 0)
            {
                driveAxis = DriveAxis.z;
            }
            else
            {
                throw new System.Exception("No drive axis for: " + name);
            }
        }
    }
}