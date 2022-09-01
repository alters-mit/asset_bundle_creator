using System.Xml.Linq;
using Pose = URDF.Pose;


namespace LISDF
{
    /// <summary>
    /// Data for including an external file.
    /// </summary>
    public struct LisdfInclude
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string name;
        /// <summary>
        /// The URI to the external file.
        /// </summary>
        public string uri;
        /// <summary>
        /// If true, this object is static (kinematic or immovable).
        /// </summary>
        public bool isStatic;
        /// <summary>
        /// The mesh scale.
        /// </summary>
        public float scale;
        /// <summary>
        /// The pose of the object.
        /// </summary>
        public Pose pose;


        public LisdfInclude(XElement element)
        {
            name = element.Attribute("name").Value.UriSafe();
            uri = element.Element("uri").Value;
            XElement staticElement = element.Element("static");
            if (staticElement != null)
            {
                isStatic = staticElement.Value.ToLower() == "true";
            }
            else
            {
                isStatic = false;
            }
            XElement scaleElement = element.Element("scale");
            if (scaleElement != null)
            {
                scale = float.Parse(scaleElement.Value);
            }
            else
            {
                scale = 1;
            }
            pose = Pose.FromPoseElement(element.Element("pose"), CoordinateSpace.ros);
        }
    }
}