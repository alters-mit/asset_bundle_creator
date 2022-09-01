using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A cylinder-shaped geometry.
    /// </summary>
    public class CylinderGeometry : PrimitiveGeometry<CapsuleCollider>
    {
        /// <summary>
        /// The length of the cylinder.
        /// </summary>
        public float length;
        /// <summary>
        /// The radius of the cylinder.
        /// </summary>
        public float radius;


        public CylinderGeometry(XElement element, float globalScale) : base(element, globalScale)
        {
            length = float.Parse(element.Attribute("length").Value);
            radius = float.Parse(element.Attribute("radius").Value);
        }


        protected override PrimitiveType GetPrimitiveType()
        {
            return PrimitiveType.Cylinder;
        }


        protected override Vector3 GetVisualScale()
        {
            return new Vector3(radius * globalScale, length * globalScale, radius * globalScale);
        }


        protected override void SetCollider(ref CapsuleCollider collider)
        {
            collider.height = length * globalScale;
            collider.radius = radius * globalScale;
        }
    }
}