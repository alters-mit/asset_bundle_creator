# CompositeObjectCreator API

CompositeObjectCreator generates articulated Unity GameObjects. Each joint of the robot has an Joint component (FixedJoint, HingeJoint, or ConfigureableJoint), visual meshes, and mesh colliders.

CompositeObjectCreator accepts .urdf source files (plus any referenced meshes).

CompositeObjectCreator is similar to RobotCreator. [Read this for more information](robot_creator_vs_composite_object_creator.md).

## API Overview

| Method                                            | Description                                                  |
| ------------------------------------------------- | ------------------------------------------------------------ |
| `CompositeObjectCreator.SourceFileToPrefab`       | From a source .urdf file plus referenced meshes, generate a .prefab file. |
| `CompositeObjectCreator.PrefabToAssetBundles`     | From a .prefab file, generate asset bundles.                 |
| `CompositeObjectCreator.CreateRecord`             | From a .prefab as well as asset bundles, generate a metadata record. |
| `CompositeObjectCreator.Cleanup`                  | Delete the `prefabs/` and `source_files/` directories (but not the output directory). |
| `CompositeObjectCreator.SourceFileToAssetBundles` | From a source .urdf file plus referenced meshes, generate a .prefab file, asset bundles, and a metadata record. This is equivalent to running `SourceFileToPrefab` + `PrefabToAssetBundles` + `CreateRecord` + `Cleanup`. |

## `CompositeObjectCreator.SourceFileToPrefab`

From a source .urdf file plus referenced meshes, generate a .prefab file. The prefab will have a Joint components, visual meshes, and [hull collider meshes](hull_mesh_colliders.md).

This can be useful if you want to:

- Generate a prefab but not asset bundles. You can then export this prefab to another project by creating a [Unity package](https://docs.unity3d.com/Manual/AssetPackages.html).
- Manually edit a prefab or check a prefab for problems before generating asset bundles.

All `collision` XML elements are ignored; CompositeObjectCreator will generated convex [hull collider meshes](hull_mesh_colliders.md).

**CompositeObjectCreator can correctly import [PartNet Mobility](https://sapien.ucsd.edu/browse) models**; if the name of the robot starts with `partnet_`, then it will imported in "PartNet Mobility space" (link positions and rotations are ignored, meshes are rotated correctly, etc.). Otherwise, the object is assumed to be in the ROS coordinate space.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod CompositeObjectCreator.SourceFileToPrefab -name="model" -source="D:/model/mobility.urdf" -output_directory="D:/asset_bundles/model"
```

### Example input and output

**Example input:**

```
model/
....mobility.urdf
....textured_objs/
........original-1.obj
........original-1.mtl
........original-2.obj
........original-2.mtl
........(etc.)
```

- Don't move any files relative to `mobility.urdf`; a .urdf file typically points to a relative path for its meshes, e.g. `textured_objects/original-1.obj`.
- The mesh files can be .dae, .fbx, .obj, etc.

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............material_0_0.mat
....source_files/
........model/
............original-1.obj
............original-1.mtl
............original-2.obj
............original-2.mtl
............original-1_colliders.obj
............original-2_colliders.obj
............(etc.)
```

- `source_files/` is the directory of files being used to create the prefab.
- `original-1_colliders.obj`and `original-1_colliders.obj` are the [hull collider meshes](hull_mesh_colliders.md) files.

**Example output in the output directory:**

```
D:/asset_bundles
....model/
........log.txt
........model.json
```

- `log.txt` is a log that will tell you if the process succeeded or failed.
- `model.json` is an intermediary file describing the .urdf file after it has been converted for usage in Unity. This includes the paths to each converted mesh file and all positions and rotations converted to Unity. This file can be useful when debugging the prefab.

### Command-line arguments

| Argument and example                         | Optional | Default | Description                                                  |
| -------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="model"`                              |          |         | The name of the generated prefab and asset bundles.          |
| `-source="D:/model/model.urdf"`              |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/model"` |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-vhacd_resolution=800000`                   | Yes      | `800000` | The [VHACD](https://github.com/kmammou/v-hacd) voxel resolution. A larger number will generate more precise hull mesh colliders but will run slower. The default value is usually what you'll want to use. |

## `CompositeObjectCreator.PrefabToAssetBundles`

From a source .prefab file, generate asset bundles for Windows, OS X, and Linux.

To use this, you must either manually create a prefab or automatically create one using `CompositeObjectCreator.SourceFileToPrefab` (see above).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod CompositeObjectCreator.PrefabToAssetBundles -name="model" -source="dummy" -output_directory="D:/asset_bundles/model"
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

| Argument and example                       | Optional | Default | Description                                                  |
| ------------------------------------------ | -------- | ------- | ------------------------------------------------------------ |
| `-name="model"`                              |          |         | The name of the generated asset bundles.                     |
| `-source="dummy"`                          |          |         | The absolute path to the source file. This isn't actually used in this method and can be set to something non-existent like `"dummy"`. |
| `-output_directory="D:/asset_bundles/model"` |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created |

## `CompositeObjectCreator.CreateRecord`

From a .prefab as well as asset bundles, generate a metadata record. [This is used in TDW,](https://github.com/threedworld-mit/tdw/blob/master/Documentation/python/librarian/robot_librarian.md) but may be useful in other applications.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod CompositeObjectCreator.CreateRecord -name="model" -source="D:/models/mobility.urdf" -output_directory="D:/asset_bundles/model"
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

- `record.json` is a JSON dictionary of the robot metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                            | Optional | Default | Description                                                  |
| ----------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="model"`                                 |          |         | The name of the generated asset bundles.                     |
| `-source="D:/models/mobility.urdf"`             |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/model"`    |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-wnid="n02883344"`                             | Yes      | `""`    | A [WordNet ID](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-wcategory="box"`                              | Yes      | `""`    | A [WordNet category](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-library_path="D:/asset_bundles/library.json"` | Yes      | `""`    | If included, this will generate a `library.json` file, a dictionary of multiple records. The data in `record.json` will be included within `library.json`. |
| `-library_description="My metadata library"`    | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |

## `CompositeObjectCreator.Cleanup`

Delete the `prefabs/` and `source_files/` directories (but not the output directory).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod CompositeObjectCreator.Cleanup -cleanup
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

## `CompositeObjectCreator.SourceFileToAssetBundles`

From a source .urdf file, generate a .prefab file, asset bundles, and a metata record. This is equivalent to running `SourceFileToPrefab` + `PrefabToAssetBundles` + `CreateRecord` + `Cleanup`.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod CompositeObjectCreator.SourceFileToAssetBundles -name="model" -source="D:/models/mobility.urdf" -output_directory="D:/asset_bundles/model"
```

### Example input and output

**Example input:**

```
model/
....mobility.urdf
....textured_objs/
........original-1.obj
........original-1.mtl
........original-2.obj
........original-2.mtl
........(etc.)
```

- Don't move any files relative to `mobility.urdf`; a .urdf file typically points to a relative path for its meshes, e.g. `textured_objects/original-1.obj`.
- The mesh files can be .dae, .fbx, .obj, etc.

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........model/
............model.prefab
............material_0_0.mat
....source_files/
........model/
............original-1.obj
............original-1.mtl
............original-2.obj
............original-2.mtl
............original-1_colliders.obj
............original-2_colliders.obj
............(etc.)
```

- `source_files/` is the directory of files being used to create the prefab.
- `original-1_colliders.obj`and `original-1_colliders.obj` are the [hull collider meshes](hull_mesh_colliders.md) files.


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
........model.json
........record.json
....library.json
```

- `Darwin/model`, `Linux/model`, and `Windows/model` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `model.json` is an intermediary file describing the .urdf file after it has been converted for usage in Unity. This includes the paths to each converted mesh file and all positions and rotations to Unity. This file can be useful when debugging the robot prefab.
- `record.json` is a JSON dictionary of the robot metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                            | Optional | Default | Description                                                  |
| ----------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="model"`                                 |          |         | The name of the generated asset bundles.                     |
| `-source="D:/models/mobility.urdf"`             |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/model"`    |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-vhacd_resolution=800000`                   | Yes      | `800000` | The [VHACD](https://github.com/kmammou/v-hacd) voxel resolution. A larger number will generate more precise hull mesh colliders but will run slower. The default value is usually what you'll want to use. |
| `-wnid="n02883344"`                             | Yes      | `""`    | A [WordNet ID](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-wcategory="box"`                              | Yes      | `""`    | A [WordNet category](https://wordnet.princeton.edu/). This is used in [TDW](https://github.com/threedworld-mit/tdw) to label types of objects. If you don't need to organize your asset bundles this way, you don't need to set this argument. |
| `-library_path="D:/asset_bundles/library.json"` | Yes      | `""`    | If included, this will generate a `library.json` file, a dictionary of multiple records. The data in `record.json` will be included within `library.json`. |
| `-library_description="My metadata library"`    | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |
| `-cleanup`                                      | Yes      |         | If included, delete the `prefabs/` and `source_files/` directories after generating the asset bundles and the metadata record. |