using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A URDF joint element.
    /// </summary>
    public abstract class UrdfJoint
    {
        /// <summary>
        /// The name of the joint.
        /// </summary>
        public string name;
        /// <summary>
        /// The parent link.
        /// </summary>
        public string parent;
        /// <summary>
        /// The child link.
        /// </summary>
        public string child;
        /// <summary>
        /// My pose.
        /// </summary>
        public Pose pose;


        public UrdfJoint(XElement element, CoordinateSpace coordinateSpace)
        {
            name = element.Attribute("name").Value;
            parent = GetLinkName(element, "parent");
            child = GetLinkName(element, "child");
            pose = Pose.FromOriginElement(element.Element("origin"), coordinateSpace);
        }


        /// <summary>
        /// Returns the name of the link.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="key">The link key.</param>
        private string GetLinkName(XElement element, string key)
        {
            XElement linkElement = element.Element(key);
            XAttribute linkAttribute = linkElement.Attribute("link");
            if (linkAttribute != null)
            {
                return linkAttribute.Value;
            }
            else
            {
                return linkElement.Value;
            }
        }
    }
}