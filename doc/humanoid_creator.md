# HumanoidCreator API

HumanoidCreator generates asset bundles of rigged humanoids from a source .fbx file.

The humanoids must already be rigged. They won't receive any colliders.

## API Overview

| Method                                           | Description                                                  |
| ------------------------------------------------ | ------------------------------------------------------------ |
| `HumanoidCreator .SourceFileToAssetBundles`      | From a source .fbx file, generate asset bundles, and a metadata record. |
| `HumanoidCreator .SourceDirectoryToAssetBundles` | From a source directory of files, generate asset bundles and metadata records. |

## `HumanoidCreator.SourceFileToAssetBundles`

From a source .fbx file, generate asset bundles, and a metadata record.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod HumanoidCreator.SourceFileToAssetBundles -name="humanoid" -source="D:/animations/humanoid.fbx" -output_directory="D:/asset_bundles/humanoids"
```

### Example input and output

**Example input:**

```
humanoids/
....humanoid.fbx
```

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........humanoid/
............humanoid.prefab
....source_files/
........humanoid/
............humanoid.fbx
```

**Example output in the output directory:**

```
D:/asset_bundles
....humanoid/
........Darwin/
............humanoid
........Linux/
............humanoid
........Windows/
............humanoid
........log.txt
........record.json
....library.json
```

- `Darwin/humanoid`, `Linux/humanoid`, and `Windows/humanoid` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                             | Optional | Default | Description                                                  |
| ------------------------------------------------ | -------- | ------- | ------------------------------------------------------------ |
| `-name="humanoid"`                               |          |         | The name of the generated asset bundles.                     |
| `-source="D:/humanoids/humanoid.anim"`           |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/humanoids"` |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created |

## `HumanoidCreator.SourceDirectoryToAssetBundles`

Generate multiple asset bundles from a directory of source files. This is always faster than repeatedly calling `SourceFileToAssetBundles`.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod HumanoidCreator.SourceDirectoryToAssetBundles -source_directory="D:/humanoids" -output_directory="D:/asset_bundles"
```

### Example input and output

**Example input:**

```
D:/humanoids/
....humanoid_0.fbx
....humanoid_1.anim
```

**Example output  in the output directory:**

```
D:/asset_bundles
....humanoid_0/
........Darwin/
............humanoid_0
........Linux/
............humanoid_0
........Windows/
............humanoid_0
........log.txt
........record.json
....humanoid_1/
........Darwin/
............humanoid_1
........Linux/
............humanoid_1
........Windows/
............humanoid_1
........log.txt
........record.json
....library.json
....progress.txt
....errors.txt
```

- `Darwin/humanoid`, `Linux/humanoid`, and `Windows/humanoid` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the animation metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.
- `progress.txt` is continuously updated; read this file to check on the current progress.
- `errors.txt` is a list of any animations that couldn't be converted into asset bundles.

### Command-line arguments

| Argument and example                         | Optional | Default | Description                                                  |
| -------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-source_directory="D:/humanoids"`           |          |         | The absolute path to the root directory of the source files. |
| `-output_directory="D:/asset_bundles"`       |          |         | The absolute path to the root output directory. If the output directory doesn't exist, it will be created. |
| `-library_description="My metadata library"` | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |
| `-overwrite`                                 | Yes      |         | If included, overwrite any existing asset bundles.           |
| `-continue_on_error`                         | Yes      |         | If included, continue to generate asset bundles if there was an error with one of the source files. |
| `-search_pattern="*.fbx"`                    | Yes      | `""`    | A search pattern for how to find source files. This method will always recursively check sub-directories. |