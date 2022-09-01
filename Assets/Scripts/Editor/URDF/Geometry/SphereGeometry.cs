using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A sphere-shaped geometry.
    /// </summary>
    public class SphereGeometry : PrimitiveGeometry<SphereCollider>
    {
        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public float radius = 1;


        public SphereGeometry(XElement element, float globalScale) : base(element, globalScale)
        {
            XElement radiusElement = element.Element("radius");
            if (radiusElement != null)
            {
                radius = float.Parse(radiusElement.Value);
            }
        }


        protected override PrimitiveType GetPrimitiveType()
        {
            return PrimitiveType.Sphere;
        }


        protected override Vector3 GetVisualScale()
        {
            float r = radius * globalScale;
            return new Vector3(r, r, r);
        }


        protected override void SetCollider(ref SphereCollider collider)
        {
            collider.radius = radius * globalScale;
        }
    }
}