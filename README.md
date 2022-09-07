# Asset Bundle Creator

This repo combines several open-source tools to make it easy to import assets into your Unity project.  You can create prefabs or [Unity3D asset bundles](https://docs.unity3d.com/Manual/AssetBundlesIntro.html) files from the following types of source files:

| Source                                                       | Output                                                       |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| .fbx or .obj                                                 | A GameObject with visual meshes, hull mesh colliders, and a Rigidbody. |
| [.urdf](http://wiki.ros.org/urdf)                            | A GameObject  with visual meshes, hull mesh colliders, and a Rigidbody, and articulated sub-objects. Each sub-object can have either a Joint component or an ArticulationBody component. |
| .anim or .fbx                                                | A .anim file (within an asset bundle).                       |
| .fbx                                                         | A rigged humanoid GameObject (without colliders).            |
| [.sdf](http://sdformat.org/spec?ver=1.9&elem=sdf) or [.lisdf](https://learning-and-intelligent-systems.github.io/kitchen-worlds/tut-lisdf/) | Multiple asset bundles of non-articulated objects, articulated objects with Joint components, and articulated objects with ArticulationBody components. |

## Why you should use this software

- All objects, articulated or otherwise, receive [hull mesh colliders](doc/hull_mesh_colliders.md), a group of convex mesh colliders generated with [VHACD](https://github.com/kmammou/v-hacd). This means that mesh colliders will always be form-fitting. See below for more information.
- All import processes can be interrupted. This can allow you to combine automatic and manual asset creation methods. You can, for example, automatically generate a prefab from a .fbx file, manually edit to the prefab, and then automatically generate asset bundles.
- [RobotCreator](doc/api/robot_creator.md) is very similar to [Unity's own URDF importer;](https://github.com/Unity-Technologies/URDF-Importer) it's a little more limited in functionality but it also tends to work better.
- This is only importer available for .lisdf files and for PartNet Mobility .urdf files.

## Why you shouldn't use this software

- This software uses command-line calls and doesn't have a user interface. A user interface will (hopefully) be added soon.
- Please review the [license](LICENSE.md).

## Requirements

- Windows, OS X, or Linux
- Unity 2020.3.24
- A valid display. On a Linux server, check your X11 settings and then export the display, e.g. `export DISPLAY=:0`.

## Usage

### With ThreeDWorld (TDW)

This software was originally made for [TDW](https://github.com/threedworld-mit/tdw), which includes helpful Python wrapper classes for the command-line calls. **TODO more**

### With Unity Editor

1. Close Unity Editor 
2. Open a terminal shell
3. Type in the command and press enter

This is an example command-line call:

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.SourceFileToPrefab -name="model" -source="D:/models/model.obj" -output_directory="D:/asset_bundles/model"
```

**All command-line calls include these arguments:**

| Argument                          | Example                                                      | Description                                                  |
| --------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| The path to the Unity executable. | `&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe"` | This example is for Windows Powershell. For OS X or Linux, replace `&` with `./` |
| `-projectPath`                    | `"C:/Users/USER/asset_bundle_creator"`                       | The path to the `asset_bundle_creator` Unity Project. Replace `USER` with your user name. |
| `-quit`                           |                                                              | This tells Unity Editor to quit after the call.              |
| `-batchmode`                      |                                                              | This tells Unity Editor to run in the background.            |
| `-executeMethod`                  | `ModelCreator.SourceFileToPrefab`                            | The name of the creator launcher and the method you want to invoke. *Note that there are no double quotes around this value.* |

All other arguments in this call such as `-name="model"` are specific to the method (in this case, `ModelCreator.SourceFileToPrefab`).

This example calls `Cleanup` which will remove all intermediary source file and prefabs from the Unity Project:

```powershell
&"C:/Program Files/Unity/Hub/Editor/2020.3.24f1/Editor/Unity.exe" -projectpath "C:/Users/USER/asset_bundle_creator" -quit -batchmode -executeMethod ModelCreator.Cleanup -cleanup
```

To learn more about the available methods and their arguments, read the documents linked in the next section:

## API Documentation

| Type of creator        | Source file types                                            | Description                                                  | API Document                                  |
| ---------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ | --------------------------------------------- |
| ModelCreator           | .fbx or .obj                                                 | Generates *non-articulated* Unity GameObjects; these GameObjects will have exactly one Rigidbody (at the root object). There may be many visual meshes and mesh colliders. | [Read this.](doc/model_creator.md)            |
| RobotCreator           | [.urdf](http://wiki.ros.org/urdf)                            | Generates *articulated* Unity robots. Each joint has an ArticulationBody component (*not* a Rigidbody), visual meshes, and mesh colliders. | [Read this.](doc/robot_creator.md)            |
| CompositeObjectCreator | [.urdf](http://wiki.ros.org/urdf)                            | Generates *articulated* Unity GameObjects. Each joint has an Rigidbody component, a Joint component (e.g. a HingeJoint),  visual meshes, and mesh colliders. | [Read this.](doc/composite_object_creator.md) |
| AnimationCreator       | .anim or .fbx                                                | Generates asset bundles of .anim files.                      | [Read this.](doc/animation_creator.md)        |
| HumanoidCreator        | .fbx                                                         | Generates asset bundles of .fbx files.                       | [Read this.](doc/hhumanoid_creator.md)        |
| LisdfReader            | [.sdf](http://sdformat.org/spec?ver=1.9&elem=sdf) or [.lisdf](https://learning-and-intelligent-systems.github.io/kitchen-worlds/tut-lisdf/) | Generates multiple GameObject asset bundles using a combination of RobotCreator and CompositeObjectCreator. | [Read this.](doc/lisdf_reader.md)             |

## Misc. Documentation

- [Hull Mesh Colliders](doc/hull_mesh_colliders.md)
- [RobotCreator vs. CompositeObjectCreator](doc/robot_creator_vs_composite_object_creator.md)

## Roadmap

- Add a Unity Editor user interface
- Add a MaterialCreator
- Add support for ROS
- Update Unity to the latest 2020.3 release