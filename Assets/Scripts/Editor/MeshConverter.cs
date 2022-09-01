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
        // We don't need to do anything extra with a .fbx file. Just copy it.
        if (source.extension == ".fbx")
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
    public static bool CreateHullCollidersMesh(SourceFile source, int vhacdResolution, float scale, out string path)
    {
        Debug.Log("Trying to get or create a collider mesh file.");
        path = source.GetPathInProjectAbsoluteWithNewName(source.filenameNoExtension + "_colliders.obj");
        if (File.Exists(path))
        {
            Debug.Log("Mesh file already exists: " + path);
            return true;
        }
        if (!File.Exists(source.originalPath))
        {
            path = "";
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
                return false;
            }
        }
        else
        {
            // Copy the .obj file.
            objPath = source.pathInProjectAbsolute;
            source.CopyToSourceFilesDirectory();
        }
        // Try to create a .wrl file.
        string wrlPath;
        if (VHACD(new SourceFile(source.filenameNoExtension, objPath, source.folderNameInProject), vhacdResolution, out wrlPath))
        {
            if (!MeshConv(wrlPath, source.filenameNoExtension + "_colliders", source.folderNameInProject, out path))
            {
                return false;
            }
        }
        else
        {
            Debug.Log("Error! Failed to create .wrl.");
            File.Copy(objPath, path);
            Debug.Log("Fallback because VHACD failed to create a .wrl: Copied " + objPath + " to " + path);
        }
        // Set the import options.
        SourceFile collidersSource = new SourceFile(Path.GetFileNameWithoutExtension(path), path, source.folderNameInProject);
        AssetDatabase.Refresh();
        AssetPostprocessor a = new AssetPostprocessor();
        a.assetPath = collidersSource.pathInProjectFromAssets;
        ModelImporter mi = (ModelImporter)a.assetImporter;
        // Re-calculate normals.
        mi.importNormals = ModelImporterNormals.Calculate;
        mi.isReadable = true;
        mi.useFileScale = false;
        mi.globalScale = scale;
        Debug.Log("Mesh scale: " + scale);
        // Apply the changes.
        AssetDatabase.ImportAsset(a.assetPath);
        AssetDatabase.Refresh();
        // Test the .obj file for unhandled PhysX errors. Fix any problems by removing .obj groups.
        HullCollidersObjFixer fixer = new HullCollidersObjFixer(collidersSource);
        fixer.Fix();
        return true;
    }


    /// <summary>
    /// Create a hull colliders GameObject.
    /// </summary>
    /// <param name="source">The colliders .obj file.</param>
    /// <param name="scale">The mesh scale.</param>
    public static GameObject[] GetColliders(SourceFile source, float scale = 1)
    {
        Mesh[] meshes = GetMeshes(source);
        List<GameObject> colliders = new List<GameObject>();
        // Duplicate each child object of the .obj to the generated hulls object.
        for (int i = 0; i < meshes.Length; i++)
        {
            // Create a new child object and parent it to the generated hulls object.
            GameObject c = new GameObject();
            c.name = "collider_" + i;
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
    /// Returns the meshes from the object's MeshFilter components.
    /// </summary>
    /// <param name="source">The source.</param>
    public static Mesh[] GetMeshes(SourceFile source)
    {
        return source.LoadAsset().GetComponentsInChildren<MeshFilter>().Select(m => m.sharedMesh).ToArray();
    }


    /// <summary>
    /// Launch assimp to convert a .fbx file to a .obj file.
    /// </summary>
    /// <param name="source">The path to the source file.</param>
    /// <param name="outputExtension">The desired output file extension.</param>
    /// <param name="path">The path to the output file.</param>
    private static bool Assimp(SourceFile source, string outputExtension, out string path)
    {
        path = source.GetPathInProjectAbsoluteWithNewExtension(outputExtension);
        LaunchExecutable("assimp", "assimp", "export \"" + source.originalPath + "\" \"" + path + "\"");
        return FileExists(path);
    }


    /// <summary>
    /// Launch VHACD to convert a .obj file to a .wrl file.
    /// </summary>
    /// <param name="source">The path to the .obj source file.</param>
    /// <param name="vhacdResolution">The voxel resolution.</param>
    /// <param name="wrlPath">The path to the .wrl output file.</param>
    private static bool VHACD(SourceFile source, int vhacdResolution, out string wrlPath)
    {
        wrlPath = source.GetPathInProjectAbsoluteWithNewExtension(".wrl");
        string logPath = PathUtil.GetPathFrom(Application.dataPath, "../vhacd_log.txt");
        LaunchExecutable("testVHACD", "vhacd",
            "--input \"" + source.originalPath + "\"" +
            " --resolution " + vhacdResolution.ToString() +
            " --output \"" + wrlPath + "\"" +
            " --log \"" + logPath + "\"");
        // Delete the log.
        if (File.Exists(logPath))
        {
            File.Delete(logPath);
        }
        return FileExists(wrlPath);
    }


    /// <summary>
    /// Launch meshconv to convert a .wrl file to a .obj file.
    /// </summary>
    /// <param name="wrlPath">The path to the .wrl source file.</param>
    /// <param name="objFilename">The name of the output file including the .obj extension.</param>
    /// <param name="objPath">The path to the .obj output file.</param>
    private static bool MeshConv(string wrlPath, string objFilename, string folderName, out string objPath)
    {
        LaunchExecutable("meshconv", "meshconv", "\"" + wrlPath + "\"" + " -c obj -o \"" + PathUtil.GetPathInUnityProjectAbsolute(Path.GetFileNameWithoutExtension(objFilename), folderName) + "\" -sg");
        objPath = PathUtil.GetPathInUnityProjectAbsolute(objFilename + ".obj", folderName);
        return FileExists(objPath);
    }


    /// <summary>
    /// Launch an executable.
    /// </summary>
    /// <param name="executableName">The executable name.</param>
    /// <param name="folderName">The name of the folder sub-directory.</param>
    /// <param name="arguments">Command line arguments.</param>
    private static void LaunchExecutable(string executableName, string folderName, string arguments)
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
        string path = Path.Combine(Directory.GetCurrentDirectory(), "executables", os, folderName, executableName);
        if (os == "Windows")
        {
            path += ".exe";
        }
        Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.Start();
        Debug.Log("Launched: " + path + " " + arguments);
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
}