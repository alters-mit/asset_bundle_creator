# LisdfReader API

LisdfReader can read [.sdf](http://sdformat.org/spec?ver=1.9&elem=sdf) and [.lisdf](https://learning-and-intelligent-systems.github.io/kitchen-worlds/tut-lisdf/) files. These *scene description* files are similar to [.urdf](http://wiki.ros.org/urdf) files but include multiple objects. .lisdf files can *reference* an external .urdf file.

LisdfReader scans .sdf and .lisdf files and generates asset bundles of each object. It also dumps a file of [TDW commands](https://github.com/threedworld-mit/tdw) that can be used to recreate the scene in TDW.

## Robots and composite objects

Asset Bundle Creator and [TDW](https://github.com/threedworld-mit/tdw) make a distinction between robots and articulated objects. Although both can be described in a .urdf file, the import process is different. Read [this](robot_creator.md) or [this](composite_object_creator.md) for more information.

It's impossible to distinguish a *robot* from an *articulated object* merely by reading a .lisdf file. To that end, the user must specify which models if any are *robots*, which will be generated with [RobotCreator](robot_creator.md). All other objects will be created with [CompositeObjectCreator](composite_object_creator).

## API Overview

| Method             | Description                                          |
| ------------------ | ---------------------------------------------------- |
| `LisdfReader.Read` | Read a .lisdf or .sdf file and create asset bundles. |

## LisdfReader.Read

Read a .lisdf or .sdf file and create asset bundles.

### Example call

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod LisdfReader.Read -source="D:/kitchen-worlds/assets/scenes/kitchen_basics.lisdf" -output_directory="D:/asset_bundles/kitchen_basics"
```

### Example input and output

**Example input:**

```
D:/kitchen-worlds/asset/
....scenes/
........kitchen_basics.lisdf
....models/
........BraiseBody/
............100693/
................mobility.urdf
................textured_objs/
....................(etc).
........(etc.)
```

- .lisdf files can reference external .urdf files; in these cases, the .urdf files and their respective meshes must be located at the expected relative paths.

**Example output:**

````
D:/asset_bundles/kitchen_basics/
....braiserbody_1/
........Darwin/
............braiserbody_1
........Linux/
............braiserbody_1
........Windows/
............braiserbody_1
........model.json
....(etc.)/
....commands.json
....log.txt
````

- `Darwin/braiserbody_1`, `Linux/braiserbody_1`, and `Windows/braiserbody_1` are the platform-specific asset bundle files.
- `model.json` is an intermediary file describing the .urdf file after it has been converted for usage in Unity. This includes the paths to each converted mesh file and all positions and rotations to Unity. This file can be useful when debugging the robot prefab.
- `commands.json` is a JSON list of [TDW commands](https://github.com/threedworld-mit/tdw) that can be used to recreate the scene in TDW.
- `log.txt` is a log of the import process.

### Command-line arguments

| Argument and example                                         | Optional | Default | Description                                                  |
| ------------------------------------------------------------ | -------- | ------- | ------------------------------------------------------------ |
| `-source="D:/kitchen-worlds/assets/scenes/kitchen_basics.lisdf"` |          |         | The absolute path to the source file.                        |
| `-output_directory="D:/asset_bundles/kitchen_basics"`        |          |         | The absolute path to the output directory. If the output directory doesn't exist, it will be created. |
| `-overwrite`                                                 | Yes      |         | If included, overwrite any asset bundles that already exist in the output directory. |
| `-cleanup`                                                   | Yes      |         | If included, delete intermediary files in `prefabs/` and `sources_files/` after creating asset bundles. |
| `-robots="{'pr2': 'D:/kitchen-worlds/assets/models/drake/pr2_description/urdf/pr2_simplified.urdf'}"` | Yes      |         | If included, this is a dictionary of robot names and corresponding paths to .urdf files. This is how LisdfReader will know which objects require [RobotCreator](robot_creator.md). Notice that the entire dictionary must be enclosed in `"double quotes"` and each string must be enclosed in `'single quotes'`. |