using UnityEditor;
using UnityEngine;
using Logger = Logging.Logger;


/// <summary>
/// Create a humanoid asset bundle from a .fbx file.
/// </summary>
public class HumanoidCreator : SourceDirectoryCreator<HumanoidCreator, HumanoidRecord>
{
    public override bool CreatePrefab()
    {
        Logger.StartLogging(logPath);
        Debug.Log("Creating a prefab.");
        source.CopyToSourceFilesDirectory();
        // Set the asset as humanoid.
        AssetPostprocessor a = new AssetPostprocessor();
        a.assetPath = source.pathInProjectFromAssets;
        ModelImporter mi = (ModelImporter)a.assetImporter;
        if (mi.animationType != ModelImporterAnimationType.Human)
        {
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            Debug.Log("Configuring importer settings...");
            // Apply the changes.
            AssetDatabase.ImportAsset(a.assetPath);
            AssetDatabase.Refresh();
            Debug.Log("...Done!");
        }
        // Instantiate the object.
        GameObject go = Object.Instantiate(source.LoadAsset());
        // Add an Animator component.
        go.GetOrAddComponent<Animator>();
        // Create the prefab.
        GameObjectToPrefab(go);
        Object.DestroyImmediate(go);
        Debug.Log("Created prefab!");
        Logger.StopLogging();
        return true;
    }


    protected override HumanoidRecord GetRecord()
    {
        return new HumanoidRecord
        {
            name = name,
            urls = GetURLs()
        };
    }
}