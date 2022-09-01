using UnityEditor;
using UnityEngine;
using Logger = Logging.Logger;


/// <summary>
/// Create a model asset bundle from a .fbx or .obj file.
/// </summary>
public class ModelCreator : SourceDirectoryCreator<ModelCreator, ModelRecord>
{
    /// <summary>
    /// The VHACD resolution.
    /// </summary>
    private readonly int vhacdResolution;
    /// <summary>
    /// If true, use the source file's internal materials.
    /// </summary>
    private readonly bool internalMaterials;
    /// <summary>
    /// The WordNet ID.
    /// </summary>
    private readonly string wnid;
    /// <summary>
    /// The WordNet category.
    /// </summary>
    private readonly string wcategory;
    /// <summary>
    /// The model scale factor.
    /// </summary>
    private readonly float scaleFactor;


    public ModelCreator(string name, string source, string outputDirectory, 
        int vhacdResolution = Constants.DEFAULT_VHACD_RESOLUTION, bool internalMaterials = false, string wnid = "", string wcategory = "", float scaleFactor = 1) : base(name, source, outputDirectory)
    {
        this.vhacdResolution = vhacdResolution;
        this.internalMaterials = internalMaterials;
        this.wnid = wnid;
        this.wcategory = wcategory;
        this.scaleFactor = scaleFactor;
    }


    public ModelCreator(string name, string source, string outputDirectory) : base(name, source, outputDirectory)
    {
        vhacdResolution = ArgumentParser.TryGet("vhacd_resolution", Constants.DEFAULT_VHACD_RESOLUTION);
        internalMaterials = ArgumentParser.GetBoolean("-internal_materials");
        wnid = ArgumentParser.TryGet("wnid", "");
        wcategory = ArgumentParser.TryGet("wcategory", "");
        scaleFactor = ArgumentParser.TryGet("scale_factor", 1);
    }


    public ModelCreator() : base()
    {
        vhacdResolution = ArgumentParser.TryGet("vhacd_resolution", Constants.DEFAULT_VHACD_RESOLUTION);
        internalMaterials = ArgumentParser.GetBoolean("-internal_materials");
        wnid = ArgumentParser.TryGet("wnid", "");
        wcategory = ArgumentParser.TryGet("wcategory", "");
        scaleFactor = ArgumentParser.TryGet("scale_factor", 1f);
    }


    public override bool CreatePrefab()
    {
        Logger.StartLogging(logPath);
        Debug.Log("Creating a prefab.");
        // Get a visual file.
        string visualPath;
        if (!MeshConverter.CreateVisualFileInSourceFilesDirectory(source, out visualPath))
        {
            return false;
        }
        SourceFile visualFile = new SourceFile(source.name, visualPath, source.name);
        // Set the import options.
        bool readable = visualFile.LoadAsset().transform.childCount == 0;
        SetObjImportOptions(visualFile, false, true, internalMaterials, readable, 1);
        // Get a .obj file to use with VHACD.
        string colliderPath;
        if (!MeshConverter.CreateHullCollidersMesh(source, vhacdResolution, 1, out colliderPath))
        {
            return false;
        }
        SourceFile colliderFile = new SourceFile(source.name, colliderPath, source.name);
        // Load the object again.
        GameObject go = Object.Instantiate(visualFile.LoadAsset());
        if (go == null)
        {
            Debug.LogError("Error! Couldn't instantiate an object at: " + visualFile.pathInProjectAbsolute);
            return false;
        }
        // Set the name of the prefab to the model name.
        go.name = name;
        // Destroy unwanted objects.
        go.DestroyAll<Camera>();
        go.DestroyAll<Light>();
        Debug.Log("Created GameObject " + go);
        // Create the hull colliders object.
        GameObject[] colliders = MeshConverter.GetColliders(colliderFile, 1);
        GameObject collidersParent = new GameObject();
        collidersParent.name = "Generated Colliders";
        foreach (GameObject c in colliders)
        {
            c.ParentAtZero(collidersParent);
        }
        // Parent the colliders.
        collidersParent.ParentAtZero(go);
        // Add a Rigidbody.
        Rigidbody r = go.GetOrAddComponent<Rigidbody>();
        GameObjectToPrefab(go);
        Debug.Log("Created prefab!");
        Logger.StopLogging();
        return true;
    }


    protected override ModelRecord GetRecord()
    {
        // Instantiate the prefab.
        GameObject go = source.InstianateFromPrefab();
        go.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        // Get the volume.
        float volume = 0;
        Collider[] colliders = go.GetComponentsInChildren<Collider>();
        // Add the volume of each collider hull.
        for (int i = 0; i < colliders.Length; i++)
        {
            RotatedBounds b = new RotatedBounds(colliders[i]);
            volume += Vector3.Distance(b.front, b.back) *
                Vector3.Distance(b.left, b.right) *
                Vector3.Distance(b.top, b.bottom);
        }
        // Append Flex data.
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        bool flex = renderers.Length == 1 && renderers[0].gameObject.Equals(go);
        ModelRecord record = new ModelRecord()
        {
            name = name,
            urls = GetURLs(),
            wnid = wnid,
            wcategory = wcategory,
            scale_factor = scaleFactor,
            do_not_use = false,
            do_not_use_reason = "",
            canonical_rotation = Vector3.zero,
            flex = flex,
            asset_bundle_sizes = new AssetBundleSizes()
            {
                Darwin = GetAssetBundleFileSize(BuildTarget.StandaloneOSX),
                Linux = GetAssetBundleFileSize(BuildTarget.StandaloneLinux64),
                Windows = GetAssetBundleFileSize(BuildTarget.StandaloneWindows64)
            },
            physics_quality = 1,
            bounds = new RotatedBounds(go),
            substructure = SubObject.GetSubstructure(go),
            composite_object = go.GetComponentsInChildren<Rigidbody>().Length > 1,
            volume = volume,
            container_shapes = new object[0]
        };
        // Destroy the object.
        Object.DestroyImmediate(go);
        return record;
    }
}