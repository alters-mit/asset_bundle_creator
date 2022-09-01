using UnityEngine;


/// <summary>
/// Bounds that is rotated around a GameObject's forward direction.
/// </summary>
public struct RotatedBounds
{

    #region FIELDS

    /// <summary>
    /// Centerpoint of the bounds.
    /// </summary>
    public Vector3 center;
    /// <summary>
    /// Point defining the front face (the one oriented towards the forward direction).
    /// </summary>
    public Vector3 front;
    /// <summary>
    /// Point defining the back face.
    /// </summary>
    public Vector3 back;
    /// <summary>
    /// Point defining the right face.
    /// </summary>
    public Vector3 right;
    /// <summary>
    /// Point defining the left face.
    /// </summary>
    public Vector3 left;
    /// <summary>
    /// Point defining the top face.
    /// </summary>
    public Vector3 top;
    /// <summary>
    /// Point defining the bottom face.
    /// </summary>
    public Vector3 bottom;

    #endregion

    #region CONSTRUCTORS

    public RotatedBounds(GameObject go)
    {
        // Save the current rotation.
        Quaternion rot = go.transform.rotation;
        // Set the rotation of the object to default.
        go.transform.eulerAngles = Vector3.zero;

        Bounds b = go.CalculateBounds();

        center = b.center;
        left = new Vector3(b.center.x - b.size.x / 2f, b.center.y, b.center.z);
        right = new Vector3(b.center.x + b.size.x / 2f, b.center.y, b.center.z);
        top = new Vector3(b.center.x, b.center.y + b.size.y / 2f, b.center.z);
        bottom = new Vector3(b.center.x, b.center.y - b.size.y / 2f, b.center.z);
        front = new Vector3(b.center.x, b.center.y, b.center.z + b.size.z / 2f);
        back = new Vector3(b.center.x, b.center.y, b.center.z - b.size.z / 2f);

        // Switch back to the correct rotation.
        go.transform.rotation = rot;
    }


    public RotatedBounds(Collider col)
    {
        Quaternion rot = col.transform.rotation;
        col.transform.rotation = Quaternion.identity;
        Bounds b = col.bounds;

        center = b.center;
        left = b.center - (col.transform.right * b.size.x / 2f);
        right = b.center + (col.transform.right * b.size.x / 2f);
        top = b.center + (col.transform.up * b.size.y / 2f);
        bottom = b.center - (col.transform.up * b.size.y / 2f);
        front = b.center + (col.transform.forward * b.size.z / 2f);
        back = b.center - (col.transform.forward * b.size.z / 2f);

        col.transform.rotation = rot;
    }

    #endregion

}
