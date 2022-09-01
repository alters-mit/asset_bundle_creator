using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A URDF visual material defined by one or more color channels.
    /// </summary>
    public class ColorChannelMaterial : UrdfMaterial
    {
        /// <summary>
        /// A list of possible channel names, in order of preference of which one we actually want to use.
        /// </summary>
        private readonly static string[] Channels = new string[] { "ambient", "diffuse", "specular", "emissive" };


        public ColorChannelMaterial(XElement element) : base(element)
        {
            Debug.Log("Trying to find color channel element.");
            bool gotColor = false;
            for (int i = 0; i < Channels.Length; i++)
            {
                XElement channelElement = element.Element(Channels[i]);
                if (channelElement != null)
                {
                    Debug.Log("Found " + Channels[i] + " element.");
                    float[] values = channelElement.Value.ToArray();
                    color = new Color(values[0], values[1], values[2], values[3]);
                    gotColor = true;
                    break;
                }
            }
            if (!gotColor)
            {
                Debug.LogWarning("Failed to find material color: " + element.Value);
                color = new Color(1, 1, 1, 1);
            }
            Debug.Log("Color: " + color);
        }


        protected override void SetMaterial(SourceFile source, ref Material material)
        {
        }
    }
}