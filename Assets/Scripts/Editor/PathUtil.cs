using System.IO;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Utility methods for paths.
/// </summary>
public static class PathUtil
{
    /// <summary>
    /// The name of the source files folder.
    /// </summary>
    private const string SOURCE_FILES_FOLDER = "source_files";
    /// <summary>
    /// The Assets folder.
    /// </summary>
    private const string ASSETS_FOLDER = "Assets";
    /// <summary>
    /// The folder for the prefabs.
    /// </summary>
    private const string PREFABS_FOLDER = "prefabs";


    /// <summary>
    /// The sources files folder location relative to Assets. Never call this directly!
    /// </summary>
    private static string sourceFilesDirectoryFromAssets;
    /// <summary>
    /// The root source file directory. Never call this directly!
    /// </summary>
    private static string sourceFilesDirectoryAbsolute;
    /// <summary>
    /// The absolute path to the prefabs directory. Never call this directly!
    /// </summary>
    private static string prefabsDirectoryAbsolute;
    /// <summary>
    /// The prefabs folder location relative to Assets. Never call this directly!
    /// </summary>
    private static string prefabsDirectoryFromAssets;
    /// <summary>
    /// The root source file directory.
    /// </summary>
    private static string SourceFilesDirectoryAbsolute
    {
        get
        {
            if (sourceFilesDirectoryAbsolute == null)
            {
                sourceFilesDirectoryAbsolute = GetFolderInAssets(SOURCE_FILES_FOLDER);
            }
            return sourceFilesDirectoryAbsolute;
        }
    }
    /// <summary>
    /// The sources files folder location relative to Assets. Never call this directory!
    /// </summary>
    private static string SourceFilesDirectoryFromAssets
    {
        get
        {
            if (sourceFilesDirectoryFromAssets == null)
            {
                // Create the directory.
                GetFolderInAssets(SOURCE_FILES_FOLDER);
                sourceFilesDirectoryFromAssets = Path.Combine(ASSETS_FOLDER, SOURCE_FILES_FOLDER).FixWindowsPath();
            }
            return sourceFilesDirectoryFromAssets;
        }
    }
    /// <summary>
    /// The prefabs folder.
    /// </summary>
    private static string PrefabsDirectoryAbsolute
    {
        get
        {
            if (prefabsDirectoryAbsolute == null)
            {
                // Create the directory.
                prefabsDirectoryAbsolute = GetFolderInAssets(PREFABS_FOLDER);
            }
            return prefabsDirectoryAbsolute;
        }
    }
    /// <summary>
    /// The prefabs folder location relative to Assets.
    /// </summary>
    private static string PrefabsDirectoryFromAssets
    {
        get
        {
            if (prefabsDirectoryFromAssets == null)
            {
                // Create the directory.
                GetFolderInAssets(PREFABS_FOLDER);
                prefabsDirectoryFromAssets = Path.Combine("Assets", PREFABS_FOLDER).FixWindowsPath();
            }
            return prefabsDirectoryFromAssets;
        }
    }


    /// <summary>
    /// Returns the absolute path to a file in the Unity Editor project, given its original path.
    /// </summary>
    /// <param name="directory">The root directory in the Unity project.</param>
    /// <param name="originalPath">The original path with a relative directory structure.</param>
    public static string GetPathInUnityProjectAbsoluteFromRelative(string directory, string originalPath)
    {
        return Path.Combine(directory, originalPath).FixWindowsPath();
    }


    /// <summary>
    /// Returns the absolute path to a file in the Unity Editor project, given its original path.
    /// </summary>
    /// <param name="originalPath">The original path.</param>
    /// <param name="folderName">The name of the folder in source_files.</param>
    public static string GetPathInUnityProjectAbsolute(string originalPath, string folderName)
    {
        // Create a sub-directory.
        string directory = Path.Combine(SourceFilesDirectoryAbsolute, folderName);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }
        return Path.Combine(directory, Path.GetFileName(originalPath)).FixWindowsPath();
    }


    /// <summary>
    /// Returns the path to a file in the Unity Editor project relative to Assets/, given its original path.
    /// </summary>
    /// <param name="originalPath">The original path.</param>
    /// <param name="folderName">The folder name in source_files.</param>
    public static string GetPathInUnityProjectFromAssets(string originalPath, string folderName)
    {
        return Path.Combine(SourceFilesDirectoryFromAssets, folderName, Path.GetFileName(originalPath)).FixWindowsPath();
    }


    /// <summary>
    /// Given a source file, return an absolute prefab path.
    /// </summary>
    /// <param name="folderName">The folder name.</param>
    /// <param name="fileName">The name of the prefab.</param>
    /// <param name="extension">The file extension.</param>
    public static string GetPrefabPathAbsolute(string folderName, string fileName, string extension = ".prefab")
    {
        // Create a sub-directory.
        string directory = Path.Combine(PrefabsDirectoryAbsolute, folderName);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }
        return Path.Combine(directory, fileName + extension).FixWindowsPath();
    }


    /// <summary>
    /// Given a source file, return a prefab path from Assets.
    /// </summary>
    /// <param name="folderName">The folder name.</param>
    /// <param name="fileName">The name of the prefab.</param>
    /// <param name="extension">The file extension.</param>
    public static string GetPrefabPathFromAssets(string folderName, string fileName, string extension = ".prefab")
    {
        return Path.Combine(PrefabsDirectoryFromAssets, folderName, fileName + extension).FixWindowsPath();
    }


    /// <summary>
    /// Returns true if the file is in the Unity Editor project.
    /// </summary>
    /// <param name="path">An absolute file path.</param>
    public static bool IsInUnityProject(string path)
    {
        return path.StartsWith(SourceFilesDirectoryAbsolute);
    }


    /// <summary>
    /// Returns the path from a root path to a relative path.
    /// </summary>
    /// <param name="from">The root directory or file path.</param>
    /// <param name="to">The relative path.</param>
    public static string GetPathFrom(string from, string to)
    {
        string p;
        if (Path.HasExtension(from))
        {
            p = Path.GetDirectoryName(from);
        }
        else
        {
            p = from;
        }
        return Path.GetFullPath(Path.Combine(p, to)).FixWindowsPath();
    }


    /// <summary>
    /// Delete temporary directories.
    /// </summary>
    public static void Cleanup()
    {
        DeleteDirectory(PrefabsDirectoryAbsolute);
        DeleteDirectory(SourceFilesDirectoryAbsolute);
        AssetDatabase.Refresh();
        // Re-create the folders.
        GetFolderInAssets(SOURCE_FILES_FOLDER);
        GetFolderInAssets(PREFABS_FOLDER);
    }


    /// <summary>
    /// Delete the directory and the .meta file.
    /// </summary>
    /// <param name="directory">The directory.</param>
    private static void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
        string file = directory + ".meta";
        if (File.Exists(file))
        {
            File.Delete(file);
        }
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
            AssetDatabase.Refresh();
        }
        return directory.FixWindowsPath();
    }
}