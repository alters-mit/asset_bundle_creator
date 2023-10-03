using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SubalternGames;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using Logger = Logging.Logger;


/// <summary>
/// Abstract base class for creating asset bundles from source files.
/// </summary>
public abstract class AssetBundleCreator<T, U>
    where T : AssetBundleCreator<T, U>, new()
    where U : Record
{
    /// <summary>
    /// The root output directory to move the asset bundles, records, etc. to.
    /// </summary>
    public readonly string outputDirectory;
    /// <summary>
    /// The log path.
    /// </summary>
    public string logPath;
    /// <summary>
    /// The name of the asset bundle.
    /// </summary>
    public string name;
    /// <summary>
    /// The source file.
    /// </summary>
    protected SourceFile source;
    /// <summary>
    /// If true, we are logging.
    /// </summary>
    private bool logging;
    /// <summary>
    /// Folder names per build target. Never call this directly!
    /// </summary>
    private static Dictionary<BuildTarget, string> buildTargetFolders;
    /// <summary>
    /// Folder names per build target.
    /// </summary>
    protected static Dictionary<BuildTarget, string> BuildTargetFolders
    {
        get
        {
            if (buildTargetFolders == null)
            {
                buildTargetFolders = new Dictionary<BuildTarget, string>();
                // Check if the user wants to build asset bundles for specific targets.
                if (ArgumentParser.GetBoolean("-linux"))
                {
                    buildTargetFolders.Add(BuildTarget.StandaloneLinux64, "Linux");
                }
                else if (ArgumentParser.GetBoolean("-osx"))
                {
                    buildTargetFolders.Add(BuildTarget.StandaloneOSX, "Darwin");
                }
                else if (ArgumentParser.GetBoolean("-windows"))
                {
                    buildTargetFolders.Add(BuildTarget.StandaloneWindows64, "Windows");
                }
                // Add WebGL as a build target.
                else if (ArgumentParser.GetBoolean("-webgl"))
                {
                    buildTargetFolders.Add(BuildTarget.WebGL, "WebGL");
                }
                // By default, build asset bundles for Linux, MacOS, and Windows.
                else
                {
                    buildTargetFolders.Add(BuildTarget.StandaloneLinux64, "Linux");
                    buildTargetFolders.Add(BuildTarget.StandaloneOSX, "Darwin");
                    buildTargetFolders.Add(BuildTarget.StandaloneWindows64, "Windows");
                }
            }
            return buildTargetFolders;
        }
    }



    public AssetBundleCreator(string name, string source, string outputDirectory)
    {
        // Fix names that will create bad URIs.
        this.name = name.UriSafe();
        this.source = new SourceFile(this.name, source, this.name);
        this.outputDirectory = outputDirectory;
        logPath = Path.Combine(outputDirectory, Constants.LOG_FILE_NAME);
    }


    public AssetBundleCreator()
    {
        name = ArgumentParser.Get("name").UriSafe();
        source = new SourceFile(name, ArgumentParser.Get("source"), name);
        outputDirectory = ArgumentParser.Get("output_directory");
        logPath = Path.Combine(outputDirectory, Constants.LOG_FILE_NAME);
    }


    /// <summary>
    /// Create a prefab from the source file.
    /// </summary>
    public abstract bool CreatePrefab();


    /// <summary>
    /// Generate the metadata record for the asset.
    /// </summary>
    /// <param name="libraryPath">The path to the library .json file. Can be null.</param>
    /// <param name="libraryDescription">The description of the library.</param>
    public void CreateRecord(string libraryPath, string libraryDescription)
    {
        Logger.StartLogging(logPath);
        U record = GetRecord();
        Debug.Log("Created record.");
        // Add the record to a library.
        if (libraryPath != "")
        {
            // Update an existing model library.
            RecordLibrary<U> library;
            if (File.Exists(libraryPath))
            {
                library = JsonWrapper.DeserializeFromPath<RecordLibrary<U>>(libraryPath);
                if (library.records.ContainsKey(record.name))
                {
                    library.records[record.name] = record;
                    Debug.Log("Updated record at: " + libraryPath);
                }
                else
                {
                    library.records.Add(record.name, record);
                    Debug.Log("Added record to: " + libraryPath);
                }
            }
            // Create a new library and add the record.
            else
            {
                library = new RecordLibrary<U>();
                library.description = libraryDescription;
                library.records = new Dictionary<string, U>();
                Debug.Log("Created library: " + libraryPath);
                library.records.Add(record.name, record);
                Debug.Log("Added record to: " + libraryPath);
            }
            JsonWrapper.Serialize(library, libraryPath, false);
        }
        // Dump the record file.
        string recordPath = Path.Combine(outputDirectory, "record.json").FixWindowsPath();
        JsonWrapper.Serialize(record, recordPath, false);
        Debug.Log("Saved record to: " + recordPath);
        Logger.StopLogging();
    }


    /// <summary>
    /// Build an asset bundle from the expected prefab name.
    /// </summary>
    public Dictionary<BuildTarget, string> CreateAssetBundles()
    {
        // Create the builds.
        AssetBundleBuild[] builds = new AssetBundleBuild[]
        {
                new AssetBundleBuild
                {
                    assetBundleName = name,
                    assetNames = new string[] { source.prefabPathFromAssets }
                }
        };
        Dictionary<BuildTarget, string> assetBundles = new Dictionary<BuildTarget, string>();
        Logger.StartLogging(logPath);
        // Create a new asset bundle for each target.
        foreach (BuildTarget target in BuildTargetFolders.Keys)
        {
            string targetDirectory = Path.Combine(outputDirectory, BuildTargetFolders[target]);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            // Build the asset bundles.
            BuildPipeline.BuildAssetBundles(targetDirectory,
                builds,
                BuildAssetBundleOptions.None,
                target);
            string assetBundlePath = Path.Combine(targetDirectory, name).FixWindowsPath();
            assetBundles.Add(target, assetBundlePath);
            Debug.Log("Created asset bundle: " + assetBundlePath);
        }
        return assetBundles;
    }


    /// <summary>
    /// Convert a single source file into a prefab.
    /// </summary>
    public static bool SourceFileToPrefab()
    {
        return new T().CreatePrefab();
    }


    /// <summary>
    /// Convert a single .prefab file into asset bundles.
    /// </summary>
    public static void PrefabToAssetBundles()
    {
        new T().CreateAssetBundles();
    }


    /// <summary>
    /// Generate a record.
    /// </summary>
    public static void CreateRecord()
    {
        new T().CreateRecord(ArgumentParser.TryGet("library_path", ""), ArgumentParser.TryGet("library_description", ""));
    }


    /// <summary>
    /// Cleanup after running the creator.
    /// </summary>
    public static void Cleanup()
    {
        if (ArgumentParser.GetBoolean("-cleanup"))
        {
            PathUtil.Cleanup();
        }
    }


    /// <summary>
    /// From a single source file, create a prefab, create asset bundles, create a record, and cleanup.
    /// </summary>
    public static void SourceFileToAssetBundles()
    {
        if (!SourceFileToPrefab())
        {
            return;
        }
        PrefabToAssetBundles();
        CreateRecord();
        Cleanup();
    }


    /// <summary>
    /// Create a prefab from a GameObject and then destroy the GameObject.
    /// </summary>
    /// <param name="go">The GameObject.</param>
    protected GameObject GameObjectToPrefab(GameObject go)
    {
        // Create the prefab.
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, source.prefabPathFromAssets);
        // Destroy the GameObject.
        Object.DestroyImmediate(go);
        return prefab;
    }


    /// <summary>
    /// Returns the file size of the asset bundle.
    /// </summary>
    /// <param name="target">The asset bundle build target.</param>
    protected int GetAssetBundleFileSize(BuildTarget target)
    {
        string path = GetAssetBundlePath(target);
        if (!File.Exists(path))
        {
            return 0;
        }
        else
        {
            return (int)new FileInfo(path).Length;
        }
    }


    /// <summary>
    /// Returns the intended URLs or file paths of the asset bundles.
    /// </summary>
    protected URLs GetURLs()
    {
        return new URLs()
        {
            Darwin = GetURI(BuildTarget.StandaloneOSX),
            Linux = GetURI(BuildTarget.StandaloneLinux64),
            Windows = GetURI(BuildTarget.StandaloneWindows64)
        };
    }



    /// <summary>
    /// Returns the metadata record for the asset bundles.
    /// </summary>
    protected abstract U GetRecord();


    /// <summary>
    /// Returns the local file path of the final output path of an asset bundle.
    /// </summary>
    /// <param name="target">The build target</param>
    protected string GetAssetBundlePath(BuildTarget target)
    {
        return Path.Combine(outputDirectory, BuildTargetFolders[target], name);
    }


    /// <summary>
    /// Fix an imported file's normals, read/write permissions, etc.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="calculateNormals">If true, calculate normals. If false, use existing normals.</param>
    /// <param name="importMaterials">If true, import materials.</param>
    /// <param name="internalMaterials">If true, use internal materials.</param>
    /// <param name="readable">If true, the mesh is readable.</param>
    /// <param name="scale">The mesh scale.</param>
    protected void SetObjImportOptions(SourceFile source, bool calculateNormals, bool importMaterials, bool internalMaterials, bool readable, float scale)
    {
        AssetPostprocessor a = new AssetPostprocessor();
        a.assetPath = source.pathInProjectFromAssets;
        ModelImporter mi = (ModelImporter)a.assetImporter;
        Debug.Log("Setting import options for: " + name);
        // Re-calculate normals.
        mi.importNormals = ModelImporterNormals.Calculate;
        // Set the material import mode.
        mi.materialImportMode = importMaterials ? ModelImporterMaterialImportMode.ImportStandard :
            ModelImporterMaterialImportMode.None;
        Debug.Log("Import mode: " + mi.materialImportMode);
        // Internally coherent.
        if (internalMaterials)
        {
            mi.materialLocation = ModelImporterMaterialLocation.InPrefab;
        }
        else
        {
            mi.materialLocation = ModelImporterMaterialLocation.External;
        }
        Debug.Log("Material location: " + mi.materialLocation);
        mi.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        mi.materialSearch = ModelImporterMaterialSearch.Local;
        mi.isReadable = readable;
        Debug.Log("Mesh readability: " + mi.isReadable);
        mi.useFileScale = false;
        mi.globalScale = scale;
        Debug.Log("Mesh scale: " + scale);
        // Apply the changes.
        AssetDatabase.ImportAsset(a.assetPath);
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// Returns the local URI file path of the final output path of an asset bundle, including the file:/// prefix.
    /// </summary>
    /// <param name="target">The build target.</param>
    private string GetURI(BuildTarget target)
    {
        return "file:///" + GetAssetBundlePath(target).FixWindowsPath();
    }
}
