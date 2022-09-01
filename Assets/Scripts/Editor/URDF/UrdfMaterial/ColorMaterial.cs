using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A URDF visual material defined by a color.
    /// </summary>
    public class ColorMaterial : UrdfMaterial
    {
        public ColorMaterial(XElement element) : base(element)
        {
            float[] values;
            XElement colorElement = element.Element("color");
            Debug.Log("Found color element.");
            XAttribute rgbaAttribute = colorElement.Attribute("rgba");
            // Use the attribute.
            if (rgbaAttribute != null)
            {
                values = rgbaAttribute.Value.ToArray();
            }
            // Use the text.
            else if (colorElement.Value != null)
            {
                values = colorElement.Value.ToArray();
            }
            else
            {
                Debug.LogWarning("Failed to find material color: " + colorElement.Value);
                values = new float[] { 1, 1, 1, 1 };
            }
            color = new Color(values[0], values[1], values[2], values[3]);
            Debug.Log("Color: " + color);
        }


        protected override void SetMaterial(SourceFile source, ref Material material)
        {
        }
    }
}