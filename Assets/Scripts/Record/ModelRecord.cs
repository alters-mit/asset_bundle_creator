using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// A model metadata record. 
/// </summary>
public class ModelRecord : Record
{
    /// <summary>
    /// The WordNet ID.
    /// </summary>
    public string wnid;
    /// <summary>
    /// The WordNet category.
    /// </summary>
    public string wcategory;
    /// <summary>
    /// The default scale factor.
    /// </summary>
    public float scale_factor;
    /// <summary>
    /// If true, do not use.
    /// </summary>
    public bool do_not_use;
    /// <summary>
    /// Do not use reason.
    /// </summary>
    public string do_not_use_reason;
    /// <summary>
    /// The canonical rotation in Euler angles.
    /// </summary>
    public Vector3 canonical_rotation;
    /// <summary>
    /// If true, this is a flex model.
    /// </summary>
    public bool flex;
    /// <summary>
    /// The physics quality.
    /// </summary>
    public float physics_quality;
    /// <summary>
    /// The sizes per platform of the asset bundles.
    /// </summary>
    public AssetBundleSizes asset_bundle_sizes;
    /// <summary>
    /// The bounds of the object.
    /// </summary>
    public RotatedBounds bounds;
    /// <summary>
    /// The object's substructure.
    /// </summary>
    public List<SubObject> substructure;
    /// <summary>
    /// Whether this is a composite object.
    /// </summary>
    public bool composite_object;
    /// <summary>
    /// The spatial volume of the object.
    /// </summary>
    public float volume;
    /// <summary>
    /// Container shapes. This should be empty.
    /// </summary>
    public object[] container_shapes;
}
