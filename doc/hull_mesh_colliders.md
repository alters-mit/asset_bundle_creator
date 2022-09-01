# Hull Mesh Colliders

In Unity, [mesh colliders will only collide with other mesh colliders if they are convex](https://docs.unity3d.com/Manual/class-MeshCollider.html). A bowl-shaped mesh, for example, is *not* convex.

In order to generate form-fitting mesh colliders, Asset Bundle Creator uses three third-party executables (included in this repo) to convert a source mesh file into *hull mesh colliders*:

1. [assimp](https://github.com/assimp/assimp) converts a mesh file to a .obj file (if it isn't already a .obj file).
2. [VHACD](https://github.com/kmammou/v-hacd) converts the .obj file to a .wrl file of hull sub-meshes.
3. [meshconv](https://www.patrickmin.com/meshconv/) converts the .wrl file back to a .obj file.

This is an example object with mesh colliders. Note that there are many mesh colliders, and when combined they conform to the visual mesh(es):

![](C:\Users\Seth Alter\asset_bundle_creator\doc\images\braiser_body_hull_colliders.gif)

All GameObject creators ([ModelCreator](model_creator.md), [RobotCreator](robot_creator.md), and [CompositeObjectCreator](composite_object_creator.md)) use this process to generate mesh colliders. In the cases of RobotCreator and CompositeObjectCreator, hull mesh colliders are generated for every joint sub-object.