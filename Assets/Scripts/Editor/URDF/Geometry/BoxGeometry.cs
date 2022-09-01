using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A box-shaped geometry.
    /// </summary>
    public class BoxGeometry : PrimitiveGeometry<BoxCollider>
    {
        /// <summary>
        /// The size of the box.
        /// </summary>
        public Vector3 size;


        public BoxGeometry(XElement element, CoordinateSpace coordinateSpace, float globalScale) : base(element, globalScale)
        {
            float[] arr;
            XElement sizeElement = element.Element("size");
            if (sizeElement != null)
            {
                arr = sizeElement.Value.ToArray();
            }
            else
            {
                XAttribute sizeAttribute = element.Attribute("size");
                if (sizeAttribute != null)
                {
                    arr = sizeAttribute.Value.ToArray();
                }
                else
                {
                    arr = new float[] { 1, 1, 1 };
                }
            }
            size = new Vector3(arr[0], arr[1], arr[2]).ScaleInCoordinateSpace(coordinateSpace);
        }


        protected override PrimitiveType GetPrimitiveType()
        {
            return PrimitiveType.Cube;
        }


        protected override Vector3 GetVisualScale()
        {
            return size * globalScale;
        }


        protected override void SetCollider(ref BoxCollider collider)
        {
            collider.size = size * globalScale;
        }
    }
}