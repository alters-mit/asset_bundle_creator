using UnityEngine;


/// <summary>
/// Extensions for GameObject.
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    /// Try to get component of type T. If it does not exist, add it.
    /// </summary>
    /// <typeparam name="T">The type of component.</typeparam> 
    /// <param name="go">(this)</param>
    public static T GetOrAddComponent<T>(this GameObject go)
        where T : Component
    {
        T o = go.GetComponent<T>();
        if (o == null)
        {
            return go.AddComponent<T>();
        }
        else
        {
            return o;
        }
    }


    /// <summary>
    /// Returns the total bounds of this gameobject's mesh and all child meshes.
    /// </summary>
    /// <param name="go">(this)</param>
    public static Bounds CalculateBounds(this GameObject go)
    {
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }


    /// <summary>
    /// Destroy all child objects that have a component of type T.
    /// </summary>
    /// <typeparam name="T">The type of component.</typeparam>
    /// <param name="go">(this)</param>
    public static void DestroyAll<T>(this GameObject go)
        where T : Component
    {
        T[] children = go.GetComponentsInChildren<T>();
        for (int i = 0; i < children.Length; i++)
        {
            Object.DestroyImmediate(children[i].gameObject);
        }
    }


    /// <summary>
    /// Parent this object. Set its local position and rotation to 0.
    /// </summary>
    /// <param name="go">(this)</param>
    /// <param name="parent">The parent.</param>
    public static void ParentAtZero(this GameObject go, Transform parent)
    {
        go.transform.parent = parent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }


    /// <summary>
    /// Parent this object. Seits local position and rotation to 0.
    /// </summary>
    /// <param name="go">(this)</param>
    /// <param name="parent">The parent.</param>
    public static void ParentAtZero(this GameObject go, GameObject parent)
    {
        ParentAtZero(go, parent.transform);
    }
}