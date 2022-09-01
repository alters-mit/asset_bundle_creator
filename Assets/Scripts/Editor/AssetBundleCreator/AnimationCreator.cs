using UnityEngine;
using UnityEditor;
using Logger = Logging.Logger;


/// <summary>
/// Create asset bundles of animation from a .anim or .fbx file.
/// </summary>
public class AnimationCreator : SourceDirectoryCreator<AnimationCreator, AnimationRecord>
{
    public AnimationCreator(string name, string source, string outputDirectory) : base(name, source, outputDirectory)
    {
        SetSource();
    }


    public AnimationCreator() : base()
    {
        SetSource();
    }


    public override bool CreatePrefab()
    {
        Logger.StartLogging(logPath);
        // Copy a .anim file.
        if (source.extension == ".anim")
        {
            source.CopyToPrefabsDirectory();
            return true;
        }
        else if (source.extension == ".fbx")
        {
            // Copy the FBX.
            source.CopyToSourceFilesDirectory();
            // Load the clip. Source: https://stackoverflow.com/a/68178683
            AnimationClip src = AssetDatabase.LoadAssetAtPath<AnimationClip>(source.pathInProjectFromAssets);
            AnimationClip dst = new AnimationClip();
            EditorUtility.CopySerialized(src, dst);
            AssetDatabase.CreateAsset(dst, PathUtil.GetPrefabPathFromAssets(name, name, extension: ".anim"));
            Debug.Log("Extracted from .fbx file: " + source.prefabPathAbsolute);
            return true;
        }
        else
        {
            Debug.LogError("Invalid source file: " + source.originalPath);
            return false;
        }
    }


    protected override AnimationRecord GetRecord()
    {
        AnimationClip animation = AssetDatabase.LoadAssetAtPath<AnimationClip>(source.prefabPathFromAssets);
        return new AnimationRecord
        {
            name = name,
            urls = GetURLs(),
            duration = animation.length,
            loop = animation.isLooping,
            framerate = (int)animation.frameRate
        };
    }


    /// <summary>
    /// Set the source file to have the correct extension.
    /// </summary>
    private void SetSource()
    {
        source = new SourceFile(source.name, source.originalPath, source.folderNameInProject, prefabExtension: ".anim");
    }
}