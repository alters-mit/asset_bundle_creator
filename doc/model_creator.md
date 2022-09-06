# ModelCreator API

ModelCreator generates *non-articulated* Unity GameObjects; these GameObjects will have exactly one Rigidbody (at the root object). There may be many visual meshes and mesh colliders.

ModelCreator accepts .fbx or .obj source files. For .obj files, it will read a .mtl if one is a available to generate materials.

## API Overview

| Method                                          | Description                                                  |
| ----------------------------------------------- | ------------------------------------------------------------ |
| `ModelCreator.SourceFileToPrefab`               | From a source .fbx or .obj file, generate a .prefab file.    |
| `ModelCreator.PrefabToAssetBundles`             | From a .prefab file, generate asset bundles.                 |
| `ModelCreator.CreateRecord`                     | From a .prefab as well as asset bundles, generate a metadata record. |
| `ModelCreator.Cleanup`                          | Delete the `prefabs/` and `source_files/` directories (but not the output directory). |
| `ModelCreator.SourceFileToAssetBundles`         | From a source .fbx or .obj file, generate a .prefab file, asset bundles, and a metata record. This is equivalent to running `SourceFileToPrefab` + `PrefabToAssetBundles` + `CreateRecord` + `Cleanup`. |
| `ModelCreator.SourceDirectoryToAssetBundles`    | Generate multiple asset bundles from a directory of source files. This is always faster than repeatedly calling `SourceFileToAssetBundles`. |
| `ModelCreator.SourceMetadataFiletoAssetBundles` | Generate multiple asset bundles from a metadata .csv file. This is similar to `SourceDirectoryToAssetBundles` but it allows you to specify metadata per file. |

## `ModelCreator.SourceFileToPrefab`

From a source .fbx or .obj file, generate a .prefab file. The prefab will have a Rigidbody, visual meshes, and [hull collider meshes](hull_mesh_colliders.md).

This can be useful if you want to:

- Generate a prefab but not asset bundles. You can then export this prefab to another project by creating a [Unity package](https://docs.unity3d.com/Manual/AssetPackages.html).
- Manually edit a prefab or check a prefab for problems before generating asset bundles.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.SourceFileToPrefab -name="model" -source="D:/models/model.obj" -output_directory="D:/asset_bundles/model"
```

### Example input and output

**Example input:**

```
D:/models/
....model.obj
....model.mtl
```

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............material.mat
....source_files/
........model/
............model.obj
............model_colliders.obj
............model.mtl
```

- `source_files/` is the directory of files being used to create the prefab.
- `material.mat` was generated from `model.mtl`.
- `model_colliders.obj` is the [hull collider meshes](hull_mesh_colliders.md) file.

**Example output in the output directory:**

```
D:/asset_bundles
....model/
........log.txt
```

- `log.txt` is a log that will tell you if the process succeeded or failed.

### Command-line arguments

| Argument and example                         | Optional | Default  | Description                                                  |
| -------------------------------------------- | -------- | -------- | ------------------------------------------------------------ |
| `-name="model"`                              |          |          | The name of the generated prefab and asset bundles.                     |
| `-source="D:/models/model.obj"`              |          |          | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/model"` |          |          | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-vhacd_resolution=800000`                   | Yes      | `800000` | The [VHACD](https://github.com/kmammou/v-hacd) voxel resolution. A larger number will generate more precise hull mesh colliders but will run slower. The default value is usually what you'll want to use. |
| `-internal_materials`                        | Yes      |          | If included, ModelCreator will assume that the source file's materials are within the file. This is only evaluated if this is a .fbx file. |

## `ModelCreator.PrefabToAssetBundles`

From a source .prefab file, generate asset bundles for Windows, OS X, and Linux.

To use this, you must either manually create a prefab or automatically create one using `ModelCreator.SourceFileToPrefab` (see above).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.PrefabToAssetBundles -name="model" -source="dummy" -output_directory="D:/asset_bundles/model"
```

### Example input and output

**Example input:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............ (etc.)
....source_files/
........model/
............ (etc.)
```

**Example output in the output directory:**

```
D:/asset_bundles
....model/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
........log.txt
```

- `Darwin/model`, `Linux/model`, and `Windows/model` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.

### Command-line arguments

| Argument and example                         | Optional | Default | Description                                                  |
| -------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="model"`                              |          |         | The name of the generated asset bundles.                     |
| `-source="dummy"`                            |          |         | The absolute path to the source file. This isn't actually used in this method and can be set to something non-existent like `"dummy"`. |
| `-output_directory="D:/asset_bundles/model"` |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created |

## `ModelCreator.CreateRecord`

From a .prefab as well as asset bundles, generate a metadata record. [This is used in TDW,](https://github.com/threedworld-mit/tdw/blob/master/Documentation/python/librarian/model_librarian.md) but may be useful in other applications.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.CreateRecord -name="model" -source="D:/models/model.obj" -output_directory="D:/asset_bundles/model"
```

### Example input and output

**Example input  in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............ (etc.)
....source_files/
........model/
............ (etc.)
```

**Example input  in the output directory:**

```
D:/asset_bundles
....model/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
```

**Example output  in the output directory:**

```
D:/asset_bundles
....model/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
........record.json
....library.json
```

- `record.json` is a JSON dictionary of the model metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                            | Optional | Default | Description                                                  |
| ----------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="model"`                                 |          |         | The name of the generated asset bundles.                     |
| `-source="D:/models/model.obj"`                 |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/model"`    |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-wnid="n02883344"`                             | Yes      | `""`    | A [WordNet ID](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-wcategory="box"`                              | Yes      | `""`    | A [WordNet category](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-scale_factor=1`                               | Yes      | 1       | This is metadata that will be appended to the record. It's unlikely that you'll ever need to set this argument. |
| `-library_path="D:/asset_bundles/library.json"` | Yes      | `""`    | If included, this will generate a `library.json` file, a dictionary of multiple records. The data in `record.json` will be included within `library.json`. |
| `-library_description="My metadata library"`    | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |

## `ModelCreator.Cleanup`

Delete the `prefabs/` and `source_files/` directories (but not the output directory).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.Cleanup -cleanup
```

### Example input and output

**Example input:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............ (etc.)
....source_files/
........model/
............ (etc.)
```

**Example result:**

```
C:/Users/USER/asset_bundle_creator
```

### Command-line arguments

| Argument and example | Optional | Default | Description                                                 |
| -------------------- | -------- | ------- | ----------------------------------------------------------- |
| `-cleanup`           | Yes      |         | This argument must be included or else nothing will happen. |

## `ModelCreator.SourceFileToAssetBundles`

From a source .fbx or .obj file, generate a .prefab file, asset bundles, and a metata record. This is equivalent to running `SourceFileToPrefab` + `PrefabToAssetBundles` + `CreateRecord` + `Cleanup`.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.SourceFileToAssetBundles -name="model" -source="D:/models/model.obj" -output_directory="D:/asset_bundles/model"
```

### Example input and output

**Example input:**

```
D:/models/
....model.obj
....model.mtl
```

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............material.mat
....source_files/
........model/
............model.obj
............model_colliders.obj
............model.mtl
```

- If the `-cleanup` argument is included in the command-line call (see below), the `prefabs/` and `source_files/` directories will be deleted after generating the asset bundles and the metadata record.
- `source_files/` is the directory of files being used to create the prefab.
- `material.mat` was generated from `model.mtl`.
- `model_colliders.obj` is the [hull collider meshes](hull_mesh_colliders.md) file.

**Example output  in the output directory:**

```
D:/asset_bundles
....model/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
........log.txt
........record.json
....library.json
```

- `Darwin/model`, `Linux/model`, and `Windows/model` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the model metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                            | Optional | Default  | Description                                                  |
| ----------------------------------------------- | -------- | -------- | ------------------------------------------------------------ |
| `-name="model"`                                 |          |          | The name of the generated asset bundles.                     |
| `-source="D:/models/model.obj"`                 |          |          | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/model"`    |          |          | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-vhacd_resolution=800000`                      | Yes      | `800000` | The [VHACD](https://github.com/kmammou/v-hacd) voxel resolution. A larger number will generate more precise hull mesh colliders but will run slower. The default value is usually what you'll want to use. |
| `-internal_materials`                           | Yes      |          | If included, ModelCreator will assume that the source file's materials are within the file. This is only evaluated if this is a .fbx file. |
| `-wnid="n02883344"`                             | Yes      | `""`     | A [WordNet ID](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-wcategory="box"`                              | Yes      | `""`     | A [WordNet category](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-scale_factor=1`                               | Yes      | 1        | This is metadata that will be appended to the record (see below). It's unlikely that you'll ever need to set this argument. |
| `-library_path="D:/asset_bundles/library.json"` | Yes      | `""`     | If included, this will generate a `library.json` file, a dictionary of multiple records. The data in `record.json` will be included within `library.json`. |
| `-library_description="My metadata library"`    | Yes      | `""`     | A description of the metadata library; this is written to `library.json`. |
| `-cleanup`                                      | Yes      |          | If included, delete the `prefabs/` and `source_files/` directories after generating the asset bundles and the metadata record. |

## `ModelCreator.SourceDirectoryToAssetBundles`

Generate multiple asset bundles from a directory of source files. This is always faster than repeatedly calling `SourceFileToAssetBundles`.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.SourceDirectoryToAssetBundles -source_directory="D:/models" -output_directory="D:/asset_bundles"
```

### Example input and output

**Example input:**

```
D:/models/
....0000/
........model.obj
........model.mtl
....0001/
........model.obj
........model.mtl
....0002/
........model.obj
........model.mtl
....(etc.)
```

**Example output  in the output directory:**

```
D:/asset_bundles
....0000/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
........log.txt
........record.json
....0001/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
........log.txt
........record.json
....0002/
........Darwin/
............model
........Linux/
............model
........Windows/
............model
........log.txt
........record.json
....(etc.)
....library.json
....progress.txt
....errors.txt
```

- `Darwin/model`, `Linux/model`, and `Windows/model` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the model metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.
- `progress.txt` is continuously updated; read this file to check on the current progress.
- `errors.txt` is a list of any models that couldn't be converted into asset bundles.

### Command-line arguments

| Argument and example                         | Optional | Default  | Description                                                  |
| -------------------------------------------- | -------- | -------- | ------------------------------------------------------------ |
| `-source_directory="D:/models"`              |          |          | The absolute path to the root directory of the source files. |
| `-output_directory="D:/asset_bundles"`       |          |          | The absolute path to the root output directory. If the output directory doesn't exist, it will be created. |
| `-library_description="My metadata library"` | Yes      | `""`     | A description of the metadata library; this is written to `library.json`. |
| `-overwrite`                                 | Yes      |          | If included, overwrite any existing asset bundles.           |
| `-continue_on_error`                         | Yes      |          | If included, continue to generate asset bundles if there was an error with one of the source files. |
| `-search_pattern="*.obj"`                    | Yes      | `""`     | A search pattern for how to find source files. This method will always recursively check sub-directories. |
| `-internal_materials`                        | Yes      |          | If included, ModelCreator will assume that the source file materials are within the file. This is only evaluated if the source file(s) are .fbx files. |
| `-vhacd_resolution=800000`                   | Yes      | `800000` | The [VHACD](https://github.com/kmammou/v-hacd) voxel resolution. A larger number will generate more precise hull mesh colliders but will run slower. The default value is usually what you'll want to use. |
| `-cleanup`                                   | Yes      |          | If included, delete the `prefabs/` and `source_files/` directories after generating the asset bundles and the metadata record. |

## `ModelCreator.MetadataFileToAssetBundles`

Generate multiple asset bundles from a metadata .csv file. This is similar to `SourceDirectoryToAssetBundles` but it allows you to specify metadata per file.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.MetadataFileToAssetBundles -metadata_path="D:/models/metadata.csv" -output_directory="D:/asset_bundles"
```

### Example input and output

**Example metadata file:**

```
name,wnid,wcategory,scale_factor,path
model_0,n04148054,scissors,1,D:/models/model_0/model_0.obj
model_1,n03056701,coaster,1,D:/models/model_1/model_1.obj
```

**Example output  in the output directory:**

```
D:/asset_bundles
....model_0/
........Darwin/
............model_0
........Linux/
............model_0
........Windows/
............model_0
........log.txt
........record.json
....model_1/
........Darwin/
............model_1
........Linux/
............model_1
........Windows/
............model_1
........log.txt
........record.json
....(etc.)
....library.json
....progress.txt
....errors.txt
```

- `Darwin/model_0`, `Linux/model_0`, and `Windows/model_0` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `record.json` is a JSON dictionary of the model metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.
- `progress.txt` is continuously updated; read this file to check on the current progress.
- `errors.txt` is a list of any models that couldn't be converted into asset bundles.

### Command-line arguments

| Argument and example                         | Optional | Default | Description                                                  |
| -------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-metadata_path="D:/models/metadata.csv"`    |          |         | The path to the metadata .csv file.                          |
| `-output_directory="D:/asset_bundles"`       |          |         | The absolute path to the root output directory. If the output directory doesn't exist, it will be created. |
| `-library_description="My metadata library"` | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |
| `-overwrite`                                 | Yes      |         | If included, overwrite any existing asset bundles.           |
| `-continue_on_error`                         | Yes      |         | If included, continue to generate asset bundles if there was an error with one of the source files. |
| `-internal_materials`                        | Yes      |          | If included, ModelCreator will assume that the source file materials are within the file. This is only evaluated if the source file(s) are .fbx files. |
| `-vhacd_resolution=800000`                   | Yes      | `800000` | The [VHACD](https://github.com/kmammou/v-hacd) voxel resolution. A larger number will generate more precise hull mesh colliders but will run slower. The default value is usually what you'll want to use. |
| `-cleanup`                                   | Yes      |          | If included, delete the `prefabs/` and `source_files/` directories after generating the asset bundles and the metadata record. |