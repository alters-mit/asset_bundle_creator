# Changelog

## 2.0.1

- Previously, if `HullCollidersFixer.CreateHullCollidersMesh()` failed to fix hull colliders (because all of the meshes are bad), it threw an exception. Now, it returns false (previously, it was `void`), and the mesh is ignored.

## 2.0.0