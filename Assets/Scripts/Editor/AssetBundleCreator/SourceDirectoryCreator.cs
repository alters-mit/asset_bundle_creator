using System;
using System.IO;
using System.Collections.Generic;
using SubalternGames;


/// <summary>
/// These creators can generate asset bundles from a source directory.
/// </summary>
public abstract class SourceDirectoryCreator<T, U> : AssetBundleCreator<T, U>
    where T: SourceDirectoryCreator<T, U>, new()
    where U: Record
{
    public SourceDirectoryCreator() : base()
    {
    }


    public SourceDirectoryCreator(string name, string source, string outputDirectory) : base(name, source, outputDirectory)
    {
    }


    /// <summary>
    /// Convert all files in a source directory to asset bundles.
    /// </summary>
    public static void SourceDirectoryToAssetBundles()
    {
        DirectoryInfo sourceDirectory = new DirectoryInfo(ArgumentParser.Get("source_directory"));
        DirectoryInfo outputDirectory = new DirectoryInfo(ArgumentParser.Get("output_directory"));
        // Create a new library.
        RecordLibrary<U> library = new RecordLibrary<U>();
        library.records = new Dictionary<string, U>();
        library.description = ArgumentParser.TryGet("library_description", "");
        // Get the library path.
        string libraryPath = Path.Combine(outputDirectory.FullName, "library.json");
        // Write the library.
        JsonWrapper.Serialize(library, libraryPath, false);
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
        // Get the files.
        string searchPattern = ArgumentParser.TryGet("search_pattern", "");
        FileInfo[] files;
        if (searchPattern == "")
        {
            files = sourceDirectory.GetFiles();
        }
        else
        {
            files = sourceDirectory.GetFiles(searchPattern, SearchOption.AllDirectories);
        }
        Dictionary<string, FileInfo> filenames = new Dictionary<string, FileInfo>();
        int fc = 0;
        foreach (FileInfo f in files)
        {
            string n = Path.GetFileNameWithoutExtension(f.Name);
            // Generate a new name.
            if (filenames.ContainsKey(n))
            {
                filenames.Add(n + "_" + fc, f);
                fc++;
            }
            // Use this filename.
            else
            {
                filenames.Add(n, f);
            }
        }
        int fileCounter = 0;
        foreach (string name in filenames.Keys)
        {
            string assetBundleDirectory = Path.Combine(outputDirectory.FullName, name);
            // Check if asset bundles exist.
            if (!overwrite)
            {
                bool assetBundlesExist = true;
                foreach (string platform in BuildTargetFolders.Values)
                {
                    string assetBundlePath = Path.Combine(assetBundleDirectory, platform, name);
                    if (!File.Exists(assetBundlePath))
                    {
                        assetBundlesExist = false;
                        break;
                    }
                }
                // Skip this asset bundle.
                if (assetBundlesExist)
                {
                    fileCounter++;
                    continue;
                }
            }
            // Cleanup anything previous.
            PathUtil.Cleanup();
            // Set the source name value.
            T creator = Activator.CreateInstance(typeof(T), new object[] { name, filenames[name].FullName, assetBundleDirectory }) as T;
            // Convert the source file to a prefab.
            bool success = creator.CreatePrefab();
            // Create asset bundles.
            creator.CreateAssetBundles();
            // Generate a record.
            creator.CreateRecord(libraryPath, library.description);
            // Log the progress.
            File.WriteAllText(progressPath, fileCounter + "\n" + files.Length + "\n" + filenames[name].FullName);
            // Something went wrong. Halt everything.
            if (!success)
            {
                // Log the error.
                File.AppendAllText(errorsPath, filenames[name].FullName + "\n");
                if (continueOnError)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }
            // Write the source.
            string sourceCitationPath = Path.Combine(assetBundleDirectory, "source.txt");
            File.WriteAllText(sourceCitationPath, filenames[name].FullName.FixWindowsPath());
            // Increment the counter.
            fileCounter++;
        }
        if (ArgumentParser.GetBoolean("-cleanup"))
        {
            PathUtil.Cleanup();
        }
    }
}