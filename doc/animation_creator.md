# AnimationCreator API

AnimationCreator generates animation asset bundles.

AnimationCreator accepts .anim and .fbx source files. If the source file is a .fbx, AnimationCreator will automatically extract the animation.

## API Overview

| Method                                           | Description                                                  |
| ------------------------------------------------ | ------------------------------------------------------------ |
| `AnimationCreator.SourceFileToAssetBundles`      | From a source .anim or .fbx file, generate asset bundles, and a metadata record. |
| `AnimationCreator.SourceDirectoryToAssetBundles` | From a source directory of files, generate asset bundles and metadata records. |

## `AnimationCreator.SourceFileToAssetBundles`

From a source .anim or .fbx file, generate asset bundles, and a metadata record.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod AnimationCreator.SourceFileToAssetBundles -name="animation" -source="D:/animations/animation.anim" -output_directory="D:/asset_bundles/animations"
```

### Example input and output

**Example input:**

```
animations/
....animation.anim
```

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........animation/
............animation.anim
```

**Example output in the output directory:**

```
D:/asset_bundles
....animation/
........Darwin/
............animation
........Linux/
............animation
........Windows/
............animation
........log.txt
........record.json
....library.json
```

- `Darwin/animation`, `Linux/animation`, and `Windows/animation` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                              | Optional | Default | Description                                                  |
| ------------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="animation"`                               |          |         | The name of the generated asset bundles.                     |
| `-source="D:/animations/animation.anim"`          |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/animations"` |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created |
| `-linux`, `-osx`, `-windows`, `-webgl`       | Yes      |          | If you add any of these flags, ModelCreator will create asset bundles for only the specified targets. You can add more than one of these flags. If you don't include any of these flags, ModelCreator defaults to making asset bundles for Linux, OSX, and Windows. |

## `AnimationCreator.SourceDirectoryToAssetBundles`

Generate multiple asset bundles from a directory of source files. This is always faster than repeatedly calling `SourceFileToAssetBundles`.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod AnimationCreator.SourceDirectoryToAssetBundles -source_directory="D:/animations" -output_directory="D:/asset_bundles"
```

### Example input and output

**Example input:**

```
D:/animations/
....animation_0.anim
....animation_1.anim
```

**Example output  in the output directory:**

```
D:/asset_bundles
....animation_0/
........Darwin/
............animation_0
........Linux/
............animation_0
........Windows/
............animation_0
........log.txt
........record.json
....animation_1/
........Darwin/
............animation_1
........Linux/
............animation_1
........Windows/
............animation_1
........log.txt
........record.json
....library.json
....progress.txt
....errors.txt
```

- `Darwin/animation`, `Linux/animation`, and `Windows/animation` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the animation metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.
- `progress.txt` is continuously updated; read this file to check on the current progress.
- `errors.txt` is a list of any animations that couldn't be converted into asset bundles.

### Command-line arguments

| Argument and example                         | Optional | Default | Description                                                  |
| -------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-source_directory="D:/animations"`          |          |         | The absolute path to the root directory of the source files. |
| `-output_directory="D:/asset_bundles"`       |          |         | The absolute path to the root output directory. If the output directory doesn't exist, it will be created. |
| `-library_description="My metadata library"` | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |
| `-overwrite`                                 | Yes      |         | If included, overwrite any existing asset bundles.           |
| `-continue_on_error`                         | Yes      |         | If included, continue to generate asset bundles if there was an error with one of the source files. |
| `-search_pattern="*.anim"`                   | Yes      | `""`    | A search pattern for how to find source files. This method will always recursively check sub-directories. |
| `-linux`, `-osx`, `-windows`, `-webgl`       | Yes      |          | If you add any of these flags, ModelCreator will create asset bundles for only the specified targets. You can add more than one of these flags. If you don't include any of these flags, ModelCreator defaults to making asset bundles for Linux, OSX, and Windows. |