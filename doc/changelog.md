# Changelog

## 2.0.2

- Fixed: Thousands of redundant calls to `HullCollidersFixer.CreateHullCollidersMesh()` if the .obj file doesn't have sub-meshes.

## 2.0.1

- Previously, if `HullCollidersFixer.CreateHullCollidersMesh()` failed to fix hull colliders (because all of the meshes are bad), it threw an exception. Now, it returns false (previously, it was `void`), and the mesh is ignored.
