using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


/// <summary>
/// The path to a source file and the path to its copy in the Unity Editor project.
/// </summary>
public struct SourceFile
{
    /// <summary>
    /// The package prefix.
    /// </summary>
    private const string PACKAGE_PREFIX = "package://";


    /// <summary>
    /// The original path to the source file.
    /// </summary>
    public string originalPath;
    /// <summary>
    /// The original file's directory.
    /// </summary>
    public string originalDirectory;
    /// <summary>
    /// The absolute path to the file in the Unity Editor project.
    /// </summary>
    public string pathInProjectAbsolute;
    /// <summary>
    /// The path to the file in the Unity Editor project starting with Assets.
    /// </summary>
    public string pathInProjectFromAssets;
    /// <summary>
    /// The absolute path to the file's directory in the Unity Editor project.
    /// </summary>
    public string directoryInProjectAbsolute;
    /// <summary>
    /// The file name.
    /// </summary>
    public string filename;
    /// <summary>
    /// The file extension.
    /// </summary>
    public string extension;
    /// <summary>
    /// The filename minus the extension.
    /// </summary>
    public string filenameNoExtension;
    /// <summary>
    /// The name of the final asset.
    /// </summary>
    public string name;
    /// <summary>
    /// The name of the source file and prefab folder.
    /// </summary>
    public string folderNameInProject;
    /// <summary>
    /// If we're going to make a prefab of this source file, this is the absolute path to the prefab.
    /// </summary>
    public string prefabPathAbsolute;
    /// <summary>
    /// The absolute path to the prefab directory.
    /// </summary>
    public string prefabDirectoryAbsolute;
    /// <summary>
    /// If we're going to make a prefab of this source file, this is the path to the prefab from Assets.
    /// </summary>
    public string prefabPathFromAssets;


    public SourceFile(string name, string originalPath, string folderNameInProject, string originalDirectory = null, string prefabExtension = ".prefab")
    {
        this.name = name;
        this.folderNameInProject = Path.GetFileNameWithoutExtension(folderNameInProject);
        // Assume this is an absolute path.
        if (originalDirectory == null)
        {
            this.originalPath = originalPath.FixWindowsPath();
        }
        // If the path starts with package://, split at the root folder and combine with the rest of the path.
        else if (originalPath.StartsWith(PACKAGE_PREFIX))
        {
            // Example: ur_description
            string rootMeshesFolder = Regex.Match(originalPath, PACKAGE_PREFIX + "(.*?)/").Groups[1].Value;
            // Example: robot_movement_interface/dependencies
            string rootSourceDirectory = Regex.Split(originalDirectory, rootMeshesFolder)[0];
            // Example: robot_movement_interface/dependencies/ur_description/meshes/ur5/visual/Base.dae
            this.originalPath = Path.Combine(rootSourceDirectory, originalPath.Replace(PACKAGE_PREFIX, "")).FixWindowsPath();
        }
        // This is a path relative to the the original directory.
        else if (originalPath.StartsWith(".."))
        {
            this.originalPath = PathUtil.GetPathFrom(originalDirectory, originalPath).FixWindowsPath();
        }
        // Assume everything is fine.
        else if (Path.IsPathRooted(originalPath))
        {
            this.originalPath = originalPath;
        }
        // Try to combine the original path and the original directory.
        else
        {
            this.originalPath = Path.Combine(originalDirectory, originalPath).FixWindowsPath();
        }
        // Get the filename.
        filename = Path.GetFileName(this.originalPath);
        filenameNoExtension = Path.GetFileNameWithoutExtension(this.originalPath);
        // The path in the project is always Assets/source_files/filename
        pathInProjectAbsolute = PathUtil.GetPathInUnityProjectAbsolute(this.originalPath, this.folderNameInProject);
        // Set the path relative to the Assets folder.
        pathInProjectFromAssets = PathUtil.GetPathInUnityProjectFromAssets(this.originalPath, this.folderNameInProject);
        prefabPathAbsolute = PathUtil.GetPrefabPathAbsolute(this.folderNameInProject, this.folderNameInProject, extension: prefabExtension);
        prefabPathFromAssets = PathUtil.GetPrefabPathFromAssets(this.folderNameInProject, this.folderNameInProject, extension: prefabExtension);
        // Set the original directory.
        this.originalDirectory = Path.GetDirectoryName(this.originalPath).FixWindowsPath();
        // Set the extension.
        extension = Path.GetExtension(this.originalPath);
        // Set the directory.
        directoryInProjectAbsolute = Path.GetDirectoryName(pathInProjectAbsolute).FixWindowsPath();
        prefabDirectoryAbsolute = Path.GetDirectoryName(prefabPathAbsolute).FixWindowsPath();
    }


    /// <summary>
    /// Returns a file path that is the same as pathInProjectAbsolute but with a new file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    public string GetPathInOriginalAbsoluteWithNewExtension(string extension)
    {
        return Path.Combine(originalDirectory, filenameNoExtension + extension).FixWindowsPath();
    }


    /// <summary>
    /// Returns a file path that is the same as pathInProjectAbsolute but with a new file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    public string GetPathInProjectAbsoluteWithNewExtension(string extension)
    {
        return Path.Combine(directoryInProjectAbsolute, filenameNoExtension + extension).FixWindowsPath();
    }


    /// <summary>
    /// Returns a file path that is the same as pathInProjectAbsolute but with a new file name and extension.
    /// </summary>
    /// <param name="name">The file name and extension.</param>
    public string GetPathInProjectAbsoluteWithNewName(string name)
    {
        return Path.Combine(directoryInProjectAbsolute, name).FixWindowsPath();
    }


    /// <summary>
    /// Copy the source file to asset_bundle_creator/Assets/source_files
    /// </summary>
    public void CopyToSourceFilesDirectory()
    {
        if (File.Exists(pathInProjectAbsolute))
        {
            return;
        }
        // Copy the file.
        File.Copy(originalPath, pathInProjectAbsolute);
        Debug.Log("Copied: " + originalPath + " to: " + pathInProjectAbsolute);
        // Refresh the asset database.
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// Copy the source file to asset_bundle_creator/Assets/prefabs.
    /// </summary>
    public void CopyToPrefabsDirectory()
    {
        // Delete the file to overwrite.
        if (File.Exists(prefabPathAbsolute))
        {
            File.Delete(prefabPathAbsolute);
        }
        // Copy the file.
        File.Copy(originalPath, prefabPathAbsolute);
        Debug.Log("Copied: " + originalPath + " to: " + prefabPathAbsolute);
        // Refresh the asset database.
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// Load the GameObject.
    /// </summary>
    public GameObject LoadAsset()
    {
        AssetDatabase.Refresh();
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(pathInProjectFromAssets);
        if (go != null)
        {
            Debug.Log("Loaded GameObject at: " + pathInProjectFromAssets);
        }
        else
        {
            Debug.LogError("Error! Failed to load GameObject at: " + pathInProjectFromAssets);
        }
        return go;
    }


    /// <summary>
    /// Instantiate an object from the prefab.
    /// </summary>
    public GameObject InstianateFromPrefab()
    {
        return Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPathFromAssets));
    }


    /// <summary>
    /// Get and if needed create a folder in Assets. Returns the full directory path.
    /// </summary>
    /// <param name="folder">The folder.</param>
    private static string GetFolderInAssets(string folder)
    {
        string directory = Path.Combine(Application.dataPath, folder);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return directory.FixWindowsPath();
    }
}