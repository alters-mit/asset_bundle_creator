using UnityEngine;
using System.Xml.Linq;


namespace LISDF
{
    /// <summary>
    /// LISDF camera data.
    /// </summary>
    public struct LisdfCamera
    {
        /// <summary>
        /// The name of the camera.
        /// </summary>
        public string name;
        /// <summary>
        /// The position of the camera.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The position that the camera will look at.
        /// </summary>
        public Vector3 lookAt;


        public LisdfCamera(XElement element)
        {
            name = element.Attribute("name").Value;
            position = element.Element("xyz").Value.ToPosition(CoordinateSpace.ros);
            lookAt = element.Element("point_to").Value.ToPosition(CoordinateSpace.ros);
        }
    }
}