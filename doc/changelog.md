# Changelog

## 2.0.6

- Downgraded to Unity 2020.3.24 to align it with TDW's version.
- Added optional flags to manually set build targets: `-linux`, `-osx`, `-windows`, and `-webgl`. By default, Asset Bundle Creator still creates asset bundles for Linux, OSX, and Windows.

## 2.0.5

- Upgraded to Unity 2020.3.48

## 2.0.4

- Fixed: Error if a URDF material name string is empty.

## 2.0.3

- Fixed: assimp on OSX doesn't have an executable flag.

## 2.0.2

- Fixed: Thousands of redundant calls to `HullCollidersFixer.CreateHullCollidersMesh()` if the .obj file doesn't have sub-meshes.

## 2.0.1

- Previously, if `HullCollidersFixer.CreateHullCollidersMesh()` failed to fix hull colliders (because all of the meshes are bad), it threw an exception. Now, it returns false (previously, it was `void`), and the mesh is ignored.
