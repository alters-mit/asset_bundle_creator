# RobotCreator API

RobotCreator generates articulated robot Unity GameObjects. Each joint of the robot has an [ArticulationBody](https://docs.unity3d.com/2020.3/Documentation/ScriptReference/ArticulationBody.html), visual meshes, and mesh colliders.

RobotCreator accepts .urdf source files (plus any referenced meshes).

RobotCreator is similar to Unity's own [URDF Importer](https://github.com/Unity-Technologies/URDF-Importer), with two key differences:

1. It works a lot better; Unity's importer often fails to convert mesh files.
2. Unity's importer adds a lot of ROS-specific components; RobotCreator doesn't.

RobotCreatoris similar to CompositeObjectCreator. [Read this for more information](robot_creator_vs_composite_object_creator.md).

## API Overview

| Method                                  | Description                                                  |
| --------------------------------------- | ------------------------------------------------------------ |
| `RobotCreator.SourceFileToPrefab`       | From a source .urdf file plus referenced meshes, generate a .prefab file. |
| `RobotCreator.PrefabToAssetBundles`     | From a .prefab file, generate asset bundles.                 |
| `RobotCreator.CreateRecord`             | From a .prefab as well as asset bundles, generate a metadata record. |
| `RobotCreator.Cleanup`                  | Delete the `prefabs/` and `source_files/` directories (but not the output directory). |
| `RobotCreator.SourceFileToAssetBundles` | From a source .urdf file plus referenced meshes, generate a .prefab file, asset bundles, and a metata record. This is equivalent to running `SourceFileToPrefab` + `PrefabToAssetBundles` + `CreateRecord` + `Cleanup`. |

## `RobotCreator.SourceFileToPrefab`

From a source .urdf file plus referenced meshes, generate a .prefab file. The prefab will have a ArticulationBody components, visual meshes, and [hull collider meshes](hull_mesh_colliders.md).

This can be useful if you want to:

- Generate a prefab but not asset bundles. You can then export this prefab to another project by creating a [Unity package](https://docs.unity3d.com/Manual/AssetPackages.html).
- Manually edit a prefab or check a prefab for problems before generating asset bundles.

All `collision` XML elements are ignored; RobotCreator will generated convex [hull collider meshes](hull_mesh_colliders.md).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod RobotCreator.SourceFileToPrefab -name="ur5" -source="D:/robot_movement_interface/dependencies/ur_description/urdf/ur5_robot.urdf" -output_directory="D:/asset_bundles/ur5"
```

### Example input and output

**Example input:**

```
ur_description/
....urdf/
........ur5_robot.urdf
....meshes/
........ur5/
............visual/
................Base.dae
................Forearm.dae
................Shoulder.dae
................UpperArm.dae
................Wrist1.dae
................Wrist3.dae
```

- Don't move any files relative to `ur5_robot.urdf`; a .urdf file typically points to a relative path for its meshes, e.g. `package://ur_description/meshes/ur5/visual/Base.dae`
- The mesh files can be .dae, .fbx, .obj, etc.

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........ur5/
............ur5.prefab
............LightGrey.mat
....source_files/
........ur5/
............Base.fbx
............Base.mtl
............Base.obj
............Base.wrl
............Base_colliders.obj
............(etc.)
```

- `source_files/` is the directory of files being used to create the prefab.
- `Base.fbx` is a converted version of `Base.dae`. Unity can't read .dae files. The file was converted automatically using [assimp](https://github.com/assimp/assimp).
- `Base.mtl`, `Base.obj`, and `Base.wrl` are intermediary files generated when creating `Base_colliders.obj`.
- `Base_colliders.obj` is the [hull collider meshes](hull_mesh_colliders.md) file.

**Example output in the output directory:**

```
D:/asset_bundles
....ur5/
........log.txt
........model.json
```

- `log.txt` is a log that will tell you if the process succeeded or failed.
- `model.json` is an intermediary file describing the .urdf file after it has been converted for usage in Unity. This includes the paths to each converted mesh file and all positions and rotations converted from ROS to Unity. This file can be useful when debugging the prefab.

### Command-line arguments

| Argument and example                              | Optional | Default | Description                                                  |
| ------------------------------------------------- | -------- | ------- | ------------------------------------------------------------ |
| `-name="ur5"`                                     |          |         | The name of the generated prefab and asset bundles.          |
| `-source="D:/ur_description/urdf/ur5_robot.urdf"` |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/ur5"`        |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-immovable`                                      | Yes      |         | If included, the robot's root object will be immovable.      |

## `RobotCreator.PrefabToAssetBundles`

From a source .prefab file, generate asset bundles for Windows, OS X, and Linux.

To use this, you must either manually create a prefab or automatically create one using `RobotCreator.SourceFileToPrefab` (see above).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod RobotCreator.PrefabToAssetBundles -name="ur5" -source="dummy" -output_directory="D:/asset_bundles/ur5"
```

### Example input and output

**Example input:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........ur5/
............ur5.prefab
............ (etc.)
....source_files/
........ur5/
............ (etc.)
```

**Example output in the output directory:**

```
D:/asset_bundles
....ur5/
........Darwin/
............ur5
........Linux/
............ur5
........Windows/
............ur5
........log.txt
```

- `Darwin/ur5`, `Linux/ur5`, and `Windows/ur5` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.

### Command-line arguments

| Argument and example                       | Optional | Default | Description                                                  |
| ------------------------------------------ | -------- | ------- | ------------------------------------------------------------ |
| `-name="ur5"`                              |          |         | The name of the generated asset bundles.                     |
| `-source="dummy"`                          |          |         | The absolute path to the source file. This isn't actually used in this method and can be set to something non-existent like `"dummy"`. |
| `-output_directory="D:/asset_bundles/ur5"` |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created |

## `RobotCreator.CreateRecord`

From a .prefab as well as asset bundles, generate a metadata record. [This is used in TDW,](https://github.com/threedworld-mit/tdw/blob/master/Documentation/python/librarian/robot_librarian.md) but may be useful in other applications.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod RobotCreator.CreateRecord -name="ur5" -source="D:/robot_movement_interface/dependencies/ur_description/urdf/ur5_robot.urdf" -output_directory="D:/asset_bundles/ur5"
```

### Example input and output

**Example input  in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........ur5/
............ur5.prefab
............ (etc.)
....source_files/
........ur5/
............ (etc.)
```

**Example input  in the output directory:**

```
D:/asset_bundles
....ur5/
........Darwin/
............ur5
........Linux/
............ur5
........Windows/
............ur5
```

**Example output  in the output directory:**

```
D:/asset_bundles
....ur5/
........Darwin/
............ur5
........Linux/
............ur5
........Windows/
............ur5
........record.json
....library.json
```

- `record.json` is a JSON dictionary of the robot metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                                         | Optional | Default | Description                                                  |
| ------------------------------------------------------------ | -------- | ------- | ------------------------------------------------------------ |
| `-name="ur5"`                                                |          |         | The name of the generated asset bundles.                     |
| `-source="D:/ur_description/urdf/ur5_robot.urdf"`            |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/ur5"`                   |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-source_description="https://github.com/ros-industrial/robot_movement_interface"` | Yes      | `""`    | A description of the source of the .urdf file. This will be added to the record metadata. |
| `-library_path="D:/asset_bundles/library.json"`              | Yes      | `""`    | If included, this will generate a `library.json` file, a dictionary of multiple records. The data in `record.json` will be included within `library.json`. |
| `-library_description="My metadata library"`                 | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |

## `RobotCreator.Cleanup`

Delete the `prefabs/` and `source_files/` directories (but not the output directory).

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod RobotCreator.Cleanup -cleanup
```

### Example input and output

**Example input:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........ur5/
............ur5.prefab
............ (etc.)
....source_files/
........ur5/
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

## `RobotCreator.SourceFileToAssetBundles`

From a source .urdf file, generate a .prefab file, asset bundles, and a metata record. This is equivalent to running `SourceFileToPrefab` + `PrefabToAssetBundles` + `CreateRecord` + `Cleanup`.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod RobotCreator.SourceFileToAssetBundles -name="ur5" -source="D:/robot_movement_interface/dependencies/ur_description/urdf/ur5_robot.urdf" -output_directory="D:/asset_bundles/ur5"
```

### Example input and output

**Example input:**

```
ur_description/
....urdf/
........ur5_robot.urdf
....meshes/
........ur5/
............visual/
................Base.dae
................Forearm.dae
................Shoulder.dae
................UpperArm.dae
................Wrist1.dae
................Wrist3.dae
```

- Don't move any files relative to `mobility.urdf`; a .urdf file typically points to a relative path for its meshes, e.g. `package://ur_description/meshes/ur5/visual/Base.dae`.
- The mesh files can be .dae, .fbx, .obj, etc.

**Example output in the Unity project:**

```
C:/Users/USER/asset_bundle_creator
....prefabs/
........ur5/
............ur5.prefab
............LightGrey.mat
....source_files/
........ur5/
............Base.fbx
............Base.mtl
............Base.obj
............Base.wrl
............Base_colliders.obj
............(etc.)
```

- - If the `-cleanup` argument is included in the command-line call (see below), the `prefabs/` and `source_files/` directories will be deleted after generating the asset bundles and the metadata record.
- `source_files/` is the directory of files being used to create the prefab.
- `Base.fbx` is a converted version of `Base.dae`. Unity can't read .dae files. The file was converted automatically using [assimp](https://github.com/assimp/assimp).
- `Base.mtl`, `Base.obj`, and `Base.wrl` are intermediary files generated when creating `Base_colliders.obj`.
- `Base_colliders.obj` is the [hull collider meshes](hull_mesh_colliders.md) file.


**Example output  in the output directory:**

```
D:/asset_bundles
....ur5/
........Darwin/
............ur5
........Linux/
............ur5
........Windows/
............ur5
........log.txt
........model.json
........record.json
....library.json
```

- `Darwin/ur5`, `Linux/ur5`, and `Windows/ur5` are the platform-specific asset bundle files.
- `log.txt` is a log that will tell you if the process succeeded or failed.
- `model.json` is an intermediary file describing the .urdf file after it has been converted for usage in Unity. This includes the paths to each converted mesh file and all positions and rotations converted from ROS to Unity. This file can be useful when debugging the robot prefab.
- `record.json` is a JSON dictionary of the robot metadata record.
- `library.json` is an optional JSON dictionary of multiple metadata records.

### Command-line arguments

| Argument and example                                         | Optional | Default | Description                                                  |
| ------------------------------------------------------------ | -------- | ------- | ------------------------------------------------------------ |
| `-name="ur5"`                                                |          |         | The name of the generated asset bundles.                     |
| `-source="D:/ur_description/urdf/ur5_robot.urdf"`            |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/ur5"`                   |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-immovable`                                                 | Yes      |         | If included, the robot's root object will be immovable.      |
| `-source_description="https://github.com/ros-industrial/robot_movement_interface"` | Yes      | `""`    | A description of the source of the .urdf file. This will be added to the record metadata. |
| `-library_path="D:/asset_bundles/library.json"`              | Yes      | `""`    | If included, this will generate a `library.json` file, a dictionary of multiple records. The data in `record.json` will be included within `library.json`. |
| `-library_description="My metadata library"`                 | Yes      | `""`    | A description of the metadata library; this is written to `library.json`. |
| `-cleanup`                                                   | Yes      |         | If included, delete the `prefabs/` and `source_files/` directories after generating the asset bundles and the metadata record. |