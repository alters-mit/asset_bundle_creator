using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Fix a hull colliders .obj file by repeatedly generating convex mesh colliders and scanning the log for error messages.
/// This removese bad groups from the .obj file.
/// </summary>
public class HullCollidersObjFixer
{
    /// <summary>
    /// The source file.
    /// </summary>
    private readonly SourceFile source;
    /// <summary>
    /// The path to the log file.
    /// </summary>
    private readonly string logPath;


    public HullCollidersObjFixer(SourceFile source)
    {
        this.source = source;
        logPath = Path.Combine(this.source.directoryInProjectAbsolute, "hull_colliders_obj_log.txt");
    }


    /// <summary>
    /// Fix the .obj file by repeatedly trying to create mesh colliders and then checking the log for errors.
    /// </summary>
    public void Fix()
    {
        bool ok = false;
        Debug.Log("Testing hull colliders file: " + source.pathInProjectAbsolute);
        int iterations = 0;
        while (!ok && iterations < 1000)
        {
            // Delete the log.
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            // Enable logging.
            Application.logMessageReceived += HandleLog;
            int count = 0;
            // Get the colliders.
            GameObject[] colliders = MeshConverter.GetColliders(new string[] { source.pathInProjectAbsolute }, ref count);
            // Stop logging.
            Application.logMessageReceived -= HandleLog;
            // Destroy the colliders.
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
            if (!File.Exists(logPath))
            {
                ok = true;
                Debug.Log("Collider hulls .obj file is ok!");
            }
            else
            {
                // Check for mesh collider errors.
                MatchCollection matches = Regex.Matches(File.ReadAllText(logPath), 
                    "Failed to create Convex Mesh from source mesh \"(.*?)\".");
                if (matches.Count == 0)
                {
                    Debug.Log("Collider hulls .obj file is ok!");
                    ok = true;
                }
                else
                {
                    // Read the .obj file.
                    string objText = File.ReadAllText(source.pathInProjectAbsolute).Replace("\r", "");
                    foreach (Match match in matches)
                    {
                        string group = match.Groups[1].Value;
                        // Remove the .obj group.
                        objText = Regex.Replace(objText, "(" + group + @"$((.|\n)*?))(g|\Z)", "g", RegexOptions.Multiline);
                        // Remove a trailing g if at the end of the file.
                        objText = Regex.Replace(objText, "g$", "", RegexOptions.Multiline);
                        Debug.Log("Removed from the .obj file: " + group);
                    }
                    // Write the text.
                    File.WriteAllText(source.pathInProjectAbsolute, objText);
                    // Refresh the asset.
                    AssetDatabase.Refresh();
                }
            }
            // Prevent an infinite loop.
            iterations++;
        }
        if (!ok)
        {
            throw new System.Exception("Failed to fix: " + source.pathInProjectAbsolute);
        }
        // Delete the log.
        if (File.Exists(logPath))
        {
            File.Delete(logPath);
        }
    }


    /// <summary>
    /// Handle log messages.
    /// </summary>
    /// <param name="logString">The log string.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <param name="type">The type of log.</param>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            File.WriteAllText(logPath, logString + "\n" + stackTrace);
        }
    }
}