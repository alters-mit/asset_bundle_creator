using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace URDF
{
    /// <summary>
    /// URDF visual material data.
    /// </summary>
    public abstract class UrdfMaterial
    {
        /// <summary>
        /// A counter for unique material names.
        /// </summary>
        private static int materialCounter = 0;
        /// <summary>
        /// The name of the material.
        /// </summary>
        public readonly string name;
        /// <summary>
        /// The material color.
        /// </summary>
        public Color color = Color.white;


        public UrdfMaterial(XElement element)
        {
            // Get the name.
            XAttribute nameAttribute = element.Attribute("name");
            if (nameAttribute != null)
            {
                name = nameAttribute.Value;
            }
            // Generate a new name.
            else
            {
                name = "material_" + materialCounter;
                materialCounter++;
            }
        }


        /// <summary>
        /// Create a new material from a color and a name.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="materials">Top-level materials (used by ReferenceMaterial). Key = The name of the material.</param>
        /// <param name="material">The material.</param>
        public virtual bool CreateMaterial(SourceFile source, Dictionary<string, UrdfMaterial> materials, out Material material)
        {
            // Set the asset path.
            string materialPath = GetMaterialPath(source.folderNameInProject);
            Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) == null)
            {
                Debug.Log("Creating a new material: " + name);
                // Create a new material.
                material = new Material(Shader.Find("Standard"));
                // Set the color.
                material.SetColor("_Color", color);
                SetMaterial(source, ref material);
                // We might need to create the directory because reference materials get created first.
                if (!Directory.Exists(source.prefabDirectoryAbsolute))
                {
                    Directory.CreateDirectory(source.prefabDirectoryAbsolute);
                    AssetDatabase.Refresh();
                }
                // Save the asset.
                AssetDatabase.CreateAsset(material, materialPath);
                Debug.Log("Created material: " + materialPath);
            }
            else
            {
                Debug.Log("Using existing material: " + name);
                material = existingMaterial;
            }
            return true;
        }


        /// <summary>
        /// Return a UrdfMaterial.
        /// </summary>
        /// <param name="element">The element.</param>
        public static UrdfMaterial Get(XElement element)
        {
            XElement colorElement = element.Element("color");
            XElement textureElement = element.Element("texture");
            if (colorElement != null)
            {
                return new ColorMaterial(element);
            }
            else if (textureElement != null)
            {
                return new TextureMaterial(element);
            }
            else if (element.Descendants().Count() > 0)
            {
                return new ColorChannelMaterial(element);
            }
            else
            {
                return new ReferenceMaterial(element);
            }
        }


        /// <summary>
        /// Do something to set the material.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="material">The material.</param>
        protected abstract void SetMaterial(SourceFile source, ref Material material);


        /// <summary>
        /// The path to the .mat file from Assets
        /// </summary>
        /// <param name="folderName">The folder name.</param>
        protected string GetMaterialPath(string folderName)
        {
            return PathUtil.GetPrefabPathFromAssets(folderName, name, extension: ".mat");
        }
    }
}