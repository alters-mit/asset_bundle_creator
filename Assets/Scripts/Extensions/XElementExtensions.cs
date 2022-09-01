using System.Xml.Linq;
using UnityEngine;


/// <summary>
/// Extensions for XElement.
/// </summary>
public static class XElementExtensions
{
    /// <summary>
    /// Converts a position attribute to a Vector3 position in Unity coordinates. Returns true if the attribute exists.
    /// </summary>
    /// <param name="element">(this)</param>
    /// <param name="vector">The position.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    /// <param name="position">The position.</param>
    public static bool GetPositionAttribute(this XElement element, CoordinateSpace coordinateSpace, out Vector3 position, string attribute = "xyz")
    {
        string value;
        if (!element.GetAttributeValue(attribute, out value))
        {
            position = default;
            return false;
        }
        position = value.ToPosition(coordinateSpace);
        return true;
    }


    /// <summary>
    /// Converts an anchor position attribute to a Vector3 position in Unity coordinates. Returns true if the attribute exists.
    /// </summary>
    /// <param name="element">(this)</param>
    /// <param name="vector">The position.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    /// <param name="position">The position.</param>
    public static bool GetAnchorPositionAttribute(this XElement element, CoordinateSpace coordinateSpace, out Vector3 position, string attribute = "xyz")
    {
        string value;
        if (!element.GetAttributeValue(attribute, out value))
        {
            position = default;
            return false;
        }
        position = value.ToAnchorPosition(coordinateSpace);
        return true;
    }


    /// <summary>
    /// Converts a rotation attribute to Euler angles in Unity coordinates. Returns true if the attribute exists.
    /// </summary>
    /// <param name="element">(this)</param>
    /// <param name="vector">The position.</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="coordinateSpace">The coordinate space.</param>
    /// <param name="rotation">The rotation.</param>
    public static bool GetEulerAnglesAttribute(this XElement element, CoordinateSpace coordinateSpace, out Vector3 rotation, string attribute = "rpy")
    {
        string value;
        if (!element.GetAttributeValue(attribute, out value))
        {
            rotation = default;
            return false;
        }
        rotation = value.ToEulerAngles(coordinateSpace);
        return true;
    }


    /// <summary>
    /// Returns an attribute value or default value.
    /// </summary>
    /// <param name="element">(this)</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="defaultValue">The default value if the attribute is missing.</param>
    public static string GetAttributeValue(this XElement element, string attribute, string defaultValue)
    {
        XAttribute xmlAttribute = element.Attribute(attribute);
        if (xmlAttribute == null)
        {
            return defaultValue;
        }
        else
        {
            return xmlAttribute.Value;
        }
    }


    /// <summary>
    /// Returns an attribute value.
    /// </summary>
    /// <param name="element">(this)</param>
    /// <param name="attribute">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    public static bool GetAttributeValue(this XElement element, string attribute, out string value)
    {
        XAttribute xmlAttribute = element.Attribute(attribute);
        if (xmlAttribute == null)
        {
            value = null;
            return false;
        }
        else
        {
            value = xmlAttribute.Value;
            return true;
        }
    }
}