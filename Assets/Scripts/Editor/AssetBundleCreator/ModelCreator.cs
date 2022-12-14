using System.IO;
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


    /// <summary>
    /// Convert all files listed in a metadata file to asset bundles.
    /// </summary>
    public static void MetadataFileToAssetBundles()
    {
        int vhacdResolution = ArgumentParser.TryGet("vhacd_resolution", Constants.DEFAULT_VHACD_RESOLUTION);
        bool internalMaterials = ArgumentParser.GetBoolean("-internal_materials");
        bool cleanup = ArgumentParser.GetBoolean("-cleanup");
        DirectoryInfo outputDirectory = new DirectoryInfo(ArgumentParser.Get("output_directory"));
        string metadata_path = ArgumentParser.Get("metadata_path");
        Logger.StartLogging(Path.Combine(outputDirectory.FullName, Constants.LOG_FILE_NAME));
        // Try to find the metadata file.
        if (!File.Exists(metadata_path))
        {
            Debug.LogError("File not found: " + metadata_path);
            return;
        }
        string[] metadata = File.ReadAllText(metadata_path).Split('\n');
        Debug.Log("Read: " + metadata_path);
        // Get a library.
        RecordLibrary<ModelRecord> library;
        string libraryPath;
        string libraryDescription;
        GetLibraryInDirectory(outputDirectory, out library, out libraryPath, out libraryDescription);
        // Continuously output the progress.
        string progressPath = Path.Combine(outputDirectory.FullName, "progress.txt");
        string errorsPath = Path.Combine(outputDirectory.FullName, "errors.txt");
        bool overwrite = ArgumentParser.GetBoolean("-overwrite");
        // Delete old progress.
        if (overwrite)
        {
            if (File.Exists(progressPath))
            {
                File.Delete(progressPath);
            }
            if (File.Exists(errorsPath))
            {
                File.Delete(errorsPath);
            }
        }
        bool continueOnError = ArgumentParser.GetBoolean("-continue_on_error");
        // Iterate through each metadata row.
        for (int i = 1; i < metadata.Length; i++)
        {
            string[] row = metadata[i].Split(',');
            // Get the model name.
            string name = row[0].Trim();
            string modelOutputDirectory = Path.Combine(outputDirectory.FullName, name);
            // Check if asset bundles exist.
            if (!overwrite)
            {
                bool assetBundlesExist = true;
                foreach (string platform in BuildTargetFolders.Values)
                {
                    string assetBundlePath = Path.Combine(modelOutputDirectory, platform, name);
                    if (!File.Exists(assetBundlePath))
                    {
                        assetBundlesExist = false;
                        break;
                    }
                }
                // Skip this asset bundle.
                if (assetBundlesExist)
                {
                    continue;
                }
            }
            string path = row[4].Trim();
            // Get a creator.
            ModelCreator creator = new ModelCreator(name, path, modelOutputDirectory,
                vhacdResolution, internalMaterials, row[1].Trim(), row[2].Trim(), float.Parse(row[3]));
            bool success = creator.CreatePrefab();
            // Something went wrong. Halt everything.
            if (!success)
            {
                File.AppendAllText(errorsPath, path + "\n");
                // Log the error.
                if (continueOnError)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }
            File.WriteAllText(progressPath, i + "\n" + metadata.Length + "\n" + path);
            creator.CreateAssetBundles();
            creator.CreateRecord(libraryPath, libraryDescription);
            if (cleanup)
            {
                PathUtil.Cleanup();
            }
        }
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
        // Create hull mesh collider files.
        string[] hullColliderPaths;
        if (!MeshConverter.CreateHullCollidersMesh(source, vhacdResolution, 1, out hullColliderPaths))
        {
            return false;
        }
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
        int count = 0;
        GameObject[] colliders = MeshConverter.GetColliders(hullColliderPaths, ref count, 1);
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