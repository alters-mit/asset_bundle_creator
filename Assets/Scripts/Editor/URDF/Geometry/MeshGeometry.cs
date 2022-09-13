using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace URDF
{
    /// <summary>
    /// A geometry defined by a mesh file.
    /// </summary>
    public class MeshGeometry : Geometry
    {
        /// <summary>
        /// The of the mesh source file.
        /// </summary>
        public SourceFile uri;
        /// <summary>
        /// The mesh scale.
        /// </summary>
        public float scale = 1;


        public MeshGeometry(string folderNameInProject, XElement element, string sourceDirectory, float globalScale) : base(element, globalScale)
        {
            if (element == null)
            {
                Debug.Log("Error! Mesh geometry element is null.");
                return;
            }
            XElement uriElement = element.Element("uri");
            string uri = uriElement == null ? element.Attribute("filename").Value : uriElement.Value;
            this.uri = new SourceFile("geometry", uri, folderNameInProject, originalDirectory: sourceDirectory);
            XElement scaleElement = element.Element("scale");
            if (scaleElement != null)
            {
                if (!float.TryParse(scaleElement.Value, out scale))
                {
                    scale = scaleElement.Value.ToArray()[0];
                }
            }
            scale *= globalScale;
        }


        public override GameObject[] GetVisuals()
        {
            string path;
            if (!MeshConverter.CreateVisualFileInSourceFilesDirectory(uri, out path))
            {
                Debug.LogError("Error! Failed to create meshes from: " + uri.originalPath);
                return null;
            }
            AssetDatabase.Refresh();
            // Set the URI.
            uri = new SourceFile(Path.GetFileNameWithoutExtension(path), path, uri.folderNameInProject);
            Debug.Log("Setting import options: " + uri.pathInProjectFromAssets);
            AssetPostprocessor a = new AssetPostprocessor();
            a.assetPath = uri.pathInProjectFromAssets;
            ModelImporter mi = (ModelImporter)a.assetImporter;
            // Re-calculate normals.
            mi.importNormals = ModelImporterNormals.Calculate;
            mi.isReadable = true;
            mi.useFileScale = false;
            mi.globalScale = scale;
            Debug.Log("Mesh scale: " + mi.globalScale);
            // Apply the changes.
            AssetDatabase.ImportAsset(a.assetPath);
            AssetDatabase.Refresh();
            // Instantiate an object from the file.
            GameObject go = Object.Instantiate(uri.LoadAsset());
            // Get the visuals from the file.
            GameObject[] visuals = go.GetComponentsInChildren<MeshRenderer>().Select(m => m.gameObject).ToArray();
            bool destroy = true;
            foreach (GameObject v in visuals)
            {
                v.transform.parent = null;
                // The root object is one of the visible meshes.
                if (v.Equals(go))
                {
                    destroy = false;
                }
            }
            // Destroy the parent.
            if (destroy)
            {
                Object.DestroyImmediate(go);
            }
            return visuals;
        }


        public override bool GetColliders(out GameObject[] colliders)
        {
            string[] paths;
            if (!MeshConverter.CreateHullCollidersMesh(uri, Constants.DEFAULT_VHACD_RESOLUTION, scale, out paths))
            {
                colliders = null;
                return false;
            }
            List<GameObject> collidersList = new List<GameObject>();
            int count = 0;
            foreach (string path in paths)
            {
                uri = new SourceFile(Path.GetFileNameWithoutExtension(path), path, uri.folderNameInProject);
                collidersList.AddRange(MeshConverter.GetColliders(new string[] { uri.pathInProjectAbsolute }, ref count, scale));
            }
            colliders = collidersList.ToArray();
            return true;
        }
    }
}
