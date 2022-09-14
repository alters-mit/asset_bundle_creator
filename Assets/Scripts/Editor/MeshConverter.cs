using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;


/// <summary>
/// Convert mesh files.
/// </summary>
public static class MeshConverter
{
    /// <summary>
    /// Create a visual mesh file in the source_files directory. Copy and modify referenced files such as .jpg textures and .mtl files.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="path">The path to the output file. This is either a .fbx or a .obj file.</param>
    public static bool CreateVisualFileInSourceFilesDirectory(SourceFile source, out string path)
    {
        Debug.Log("Trying to get or create a visual mesh file.");
        // The source file doesn't exist.
        if (!File.Exists(source.originalPath))
        {
            path = "";
            Debug.LogError("Error! File doesn't exist: " + source.originalPath);
            return false;
        }
        // We don't need to do anything extra with a .fbx or .dae file. Just copy it.
        if (source.extension == ".fbx" || source.extension == ".dae")
        {
            source.CopyToSourceFilesDirectory();
            path = source.pathInProjectAbsolute;
            return true;
        }
        // For a .obj file, there might be .mtl file.
        else if (source.extension == ".obj")
        {
            // Copy the .obj file.
            source.CopyToSourceFilesDirectory();
            path = source.pathInProjectAbsolute;
            SourceFile mtl = new SourceFile(source.filenameNoExtension, 
                source.GetPathInOriginalAbsoluteWithNewExtension(".mtl"),
                source.folderNameInProject);
            if (File.Exists(mtl.originalPath))
            {
                Debug.Log("Found .mtl file: " + mtl.originalPath);
                // Read the .mtl file.
                string mtlText = File.ReadAllText(mtl.originalPath);
                // Find all references to images.
                MatchCollection mtlImagePathMatches = Regex.Matches(mtlText, @"newmtl((.|\n)*?) ((.*?)\.(jpg|png))");
                foreach (Match match in mtlImagePathMatches)
                {
                    // For example: ../images/texture0.jpg
                    string mtlImagePath = match.Groups[3].Value;
                    Debug.Log("Found a reference to an image in the .mtl file: " + mtlImagePath);
                    string absoluteImageSourcePath = PathUtil.GetPathFrom(source.originalDirectory, mtlImagePath);
                    // The image doesn't exist.
                    if (!File.Exists(absoluteImageSourcePath))
                    {
                        Debug.LogError("Error! Couldn't find image: " + absoluteImageSourcePath);
                        return false;
                    }
                    // For example: images/texture0.jpg
                    string nonRelativeImagePath = Regex.Match(mtlImagePath, @"(\w+(.*))").Groups[1].Value;
                    // Get the destination path.
                    string absoluteImageDestinationPath = PathUtil.GetPathInUnityProjectAbsoluteFromRelative(source.directoryInProjectAbsolute, nonRelativeImagePath);
                    if (!File.Exists(absoluteImageDestinationPath))
                    {
                        string absoluteImageDestinationDirectory = Path.GetDirectoryName(absoluteImageDestinationPath);
                        // Create the directory.
                        if (!Directory.Exists(absoluteImageDestinationDirectory))
                        {
                            Directory.CreateDirectory(absoluteImageDestinationDirectory);
                        }
                        // Copy the image file.
                        File.Copy(absoluteImageSourcePath, absoluteImageDestinationPath);
                        Debug.Log("Copied: " + absoluteImageSourcePath + " to: " + absoluteImageDestinationPath);
                    }
                    // Update the .mtl text.
                    mtlText = mtlText.Replace(mtlImagePath, nonRelativeImagePath);
                }
                // Write the .mtl file.
                File.WriteAllText(mtl.pathInProjectAbsolute, mtlText);
                Debug.Log("Wrote a modified .mtl file to: " + mtl.pathInProjectAbsolute);
            }
            AssetDatabase.Refresh();
            return true;
        }
        // Try to convert the file.
        else
        {
            return Assimp(source, ".fbx", out path);
        }
    }


    /// <summary>
    /// Create a hull colliders .obj file from a source file.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="vhacdResolution">The VHACD resoution.</param>
    /// <param name="path">The path to the .obj hull colliders file.</param>
    /// <param name="scale">The mesh scale.</param>
    public static bool CreateHullCollidersMesh(SourceFile source, int vhacdResolution, float scale, out string[] hullColliderPaths)
    {
        Debug.Log("Trying to get or create a collider mesh file.");
        if (!File.Exists(source.originalPath))
        {
            hullColliderPaths = new string[0];
            Debug.LogError("Error! Mesh file doesn't exist: " + source.originalPath);
            return false;
        }
        string objPath;
        // Convert the source file to a .obj file using assimp.
        if (source.extension != ".obj")
        {
            Debug.Log("Source file is a: " + source.extension + "... Converting now to a .obj...");
            if (!Assimp(source, ".obj", out objPath))
            {
                hullColliderPaths = new string[0];
                return false;
            }
        }
        else
        {
            // Copy the .obj file.
            objPath = source.pathInProjectAbsolute;
            source.CopyToSourceFilesDirectory();
            hullColliderPaths = new string[] { source.pathInProjectAbsolute };
        }
        // Try to create a decomp.obj file.
        if (!VHACD(new SourceFile(source.filenameNoExtension, objPath, source.folderNameInProject), vhacdResolution, out hullColliderPaths))
        {
            Debug.Log("Error! Failed to create hull colliders.");
            SourceFile objSource = new SourceFile(source.name, objPath, source.folderNameInProject);
            objSource.CopyToSourceFilesDirectory();
            hullColliderPaths = new string[] { objSource.pathInProjectAbsolute };
        }
        // Set the import options for each hull collider.
        for (int i = 0; i < hullColliderPaths.Length; i++)
        {
            SourceFile collidersSource = new SourceFile(Path.GetFileNameWithoutExtension(hullColliderPaths[i]), hullColliderPaths[i], source.folderNameInProject);
            AssetDatabase.Refresh();
            AssetPostprocessor a = new AssetPostprocessor();
            a.assetPath = collidersSource.pathInProjectFromAssets;
            ModelImporter mi = (ModelImporter)a.assetImporter;
            // Re-calculate normals.
            mi.importNormals = ModelImporterNormals.Calculate;
            mi.isReadable = true;
            mi.useFileScale = false;
            mi.globalScale = scale;
            // Apply the changes.
            AssetDatabase.ImportAsset(a.assetPath);
            AssetDatabase.Refresh();
            // Test the .obj file for unhandled PhysX errors. Fix any problems by removing .obj groups.
            HullCollidersObjFixer fixer = new HullCollidersObjFixer(collidersSource);
            fixer.Fix();
        }
        return true;
    }


    /// <summary>
    /// Create a hull colliders GameObject.
    /// </summary>
    /// <param name="scale">The mesh scale.</param>
    /// <param name="paths">The paths to the .obj hull collider files.</param>
    /// <param name="count">The collider counter.</param>
    public static GameObject[] GetColliders(string[] paths, ref int count, float scale = 1)
    {
        List<Mesh> meshes = new List<Mesh>();
        foreach (string path in paths)
        {
            string pathFromAssets = "Assets" + Regex.Split(path, "Assets")[1];
            meshes.AddRange(AssetDatabase.LoadAssetAtPath<GameObject>(pathFromAssets).GetComponentsInChildren<MeshFilter>().Select(m => m.sharedMesh));
        }
        List<GameObject> colliders = new List<GameObject>();
        // Duplicate each child object of the .obj to the generated hulls object.
        for (int i = 0; i < meshes.Count; i++)
        {
            // Create a new child object and parent it to the generated hulls object.
            GameObject c = new GameObject();
            c.name = "collider_" + count;
            count++;
            // Add a MeshCollider and apply the mesh.
            MeshCollider mc = c.AddComponent<MeshCollider>();
            mc.convex = true;
            mc.sharedMesh = meshes[i];
            colliders.Add(c);
        }
        Debug.Log("Added Generated Colliders GameObject.");
        return colliders.ToArray();
    }


    /// <summary>
    /// Launch assimp to convert a .fbx file to a .obj file.
    /// </summary>
    /// <param name="source">The path to the source file.</param>
    /// <param name="outputExtension">The desired output file extension.</param>
    /// <param name="path">The path to the output file.</param>
    private static bool Assimp(SourceFile source, string outputExtension, out string path)
    {
        string output;
        path = source.GetPathInProjectAbsoluteWithNewExtension(outputExtension);
        LaunchExecutable("assimp", "assimp",
            "export \"" + source.originalPath + "\" \"" + path + "\"",
            false,
            out output);
        return FileExists(path);
    }


    /// <summary>
    /// Launch VHACD to convert a .obj file to a .wrl file.
    /// </summary>
    /// <param name="source">The path to the .obj source file.</param>
    /// <param name="vhacdResolution">The voxel resolution.</param>
    /// <param name="objPaths">The paths to the .obj hull colliderfiles.</param>
    private static bool VHACD(SourceFile source, int vhacdResolution, out string[] objPaths)
    {
        string output;
        // Get the path to the VHACD log file.
        // Set the source file. Set the voxel resolution. Set obj as the export file type. Disable async. Disable logging.
        LaunchExecutable("testVHACD", "vhacd",
            "\"" + source.originalPath + "\"" +
            " -r " + vhacdResolution.ToString() +
            " -o obj -a false",
            true,
            out output);
        // Delete the decomp files.
        string decompStlPath = Path.Combine(GetExecutableDirectory("vhacd"), "decomp.stl");
        if (File.Exists(decompStlPath))
        {
            File.Delete(decompStlPath);
        }
        string decompObjPath = Path.Combine(GetExecutableDirectory("vhacd"), "decomp.obj");
        if (File.Exists(decompObjPath))
        {
            File.Delete(decompObjPath);
        }
        // Get the paths of each hull mesh.
        MatchCollection matches = Regex.Matches(output, "Saving:(.*)");
        objPaths = new string[matches.Count];
        if (matches.Count == 0)
        {
            Debug.LogError("Failed to create hull colliders!");
            return false;
        }
        for (int i = 0; i < objPaths.Length; i++)
        {
            objPaths[i] = matches[i].Groups[1].Value.Trim();
        }
        AssetDatabase.Refresh();
        return true;
    }


    /// <summary>
    /// Launch an executable.
    /// </summary>
    /// <param name="executableName">The executable name.</param>
    /// <param name="folderName">The name of the folder sub-directory.</param>
    /// <param name="arguments">Command line arguments.</param>
    /// <param name="stdout">If true, redirect stdout.</param>
    /// <param name="output">The output from stdout.</param>
    private static void LaunchExecutable(string executableName, string folderName, string arguments, bool stdout, out string output)
    {
        // Get the path to the executable.
        string path = Path.Combine(GetExecutableDirectory(folderName), executableName);
        // Add a file extension.
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            path += ".exe";
        }
        Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = !stdout;
        process.StartInfo.RedirectStandardOutput = stdout;
        if (stdout)
        {
            process.StartInfo.CreateNoWindow = true;
        }
        else
        {
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        }
        process.Start();
        Debug.Log("Launched: " + path + " " + arguments);
        if (stdout)
        {
            output = process.StandardOutput.ReadToEnd();
        }
        else
        {
            output = "";
        }
        process.WaitForExit();
        Debug.Log("Done!");
    }


    /// <summary>
    /// Returns true if the file exists.
    /// </summary>
    /// <param name="path">The file path.</param>
    private static bool FileExists(string path)
    {
        // Check if the process created a .obj file.
        if (File.Exists(path))
        {
            Debug.Log("Created: " + path);
            return true;
        }
        else
        {
            Debug.LogError("Error! Failed to create: " + path);
            return false;
        }
    }


    /// <summary>
    /// Returns the path to the executable's directory.
    /// </summary>
    /// <param name="folderName">The executable's folder.</param>
    private static string GetExecutableDirectory(string folderName)
    {
        string os;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            os = "Windows";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            os = "Darwin";
        }
        else
        {
            os = "Linux";
        }
        return Path.Combine(Directory.GetCurrentDirectory(), "executables", os, folderName);
    }
}
