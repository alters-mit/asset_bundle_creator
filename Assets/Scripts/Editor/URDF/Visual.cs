using System.Xml.Linq;
using UnityEngine;
using Newtonsoft.Json;


namespace URDF
{
    /// <summary>
    /// A URDF visual element.
    /// </summary>
    public struct Visual
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string name;
        /// <summary>
        /// The visual geometry.
        /// </summary>
        public Geometry geometry;
        /// <summary>
        /// The visual material.
        /// </summary>
        public UrdfMaterial material;
        /// <summary>
        /// The visual pose.
        /// </summary>
        public Pose pose;


        public Visual(XElement element, string sourceDirectory,  string folderNameInProject, CoordinateSpace coordinateSpace, float globalScale)
        {
            name = element.GetAttributeValue("name", "visual");
            geometry = Geometry.Get(element.Element("geometry"), sourceDirectory, folderNameInProject, coordinateSpace, globalScale);
            XElement materialElement = element.Element("material");
            // Get the material from a node.
            if (materialElement != null)
            {
                material = UrdfMaterial.Get(materialElement);
            }
            else
            {
                material = null;
            }
            pose = Pose.FromOriginElement(element.Element("origin"), coordinateSpace);
        }
    }
}