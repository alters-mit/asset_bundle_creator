using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// An object in the substructure.
/// </summary>
public class SubObject
{
    /// <summary>
    /// The name of the object.
    /// </summary>
    public string name;
    /// <summary>
    /// The names of the visual materials.
    /// </summary>
    public string[] materials;


    public SubObject(string name, string[] materials)
    {
        this.name = name;
        this.materials = materials;
    }


    /// <summary>
    /// Get the visual material substructure of the object.
    /// </summary>
    /// <param name="go">The object.</param>
    public static List<SubObject> GetSubstructure(GameObject go)
    {
        List<SubObject> substructure = new List<SubObject>();

        // Create the substructure metadata.
        foreach (MeshRenderer r in go.GetComponentsInChildren<MeshRenderer>())
        {
            string[] materials = new string[r.sharedMaterials.Length];
            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                materials[i] = r.sharedMaterials[i].name;
            }
            substructure.Add(new SubObject(r.name.Replace("(Clone)", ""), materials));
        }

        return substructure;
    }
}