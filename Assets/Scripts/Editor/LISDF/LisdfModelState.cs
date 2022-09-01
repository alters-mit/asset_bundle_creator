using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace LISDF
{
    /// <summary>
    /// An LISDF model state.
    /// </summary>
    public struct LisdfModelState
    {
        /// <summary>
        /// The name of the model.
        /// </summary>
        public string name;
        /// <summary>
        /// Joint angles per joint name.
        /// </summary>
        public Dictionary<string, float> angles;


        public LisdfModelState(XElement element)
        {
            name = element.Attribute("name").Value.UriSafe();
            IEnumerable<XElement> jointElements = element.Elements("joint");
            angles = new Dictionary<string, float>();
            foreach (XElement jointElement in jointElements)
            {
                angles.Add(jointElement.Attribute("name").Value, Mathf.Rad2Deg * float.Parse(jointElement.Element("angle").Value));
            }
        }
    }
}