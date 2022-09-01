using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


namespace URDF
{
    /// <summary>
    /// A material with a texture.
    /// </summary>
    public class TextureMaterial : UrdfMaterial
    {
        /// <summary>
        /// The relative texture path.
        /// </summary>
        public string relativeTexturePath;


        public TextureMaterial(XElement element) : base(element)
        {
            XElement textureElement = element.Element("texture");
            relativeTexturePath = textureElement.Attribute("filename").Value;
        }


        protected override void SetMaterial(SourceFile source, ref Material material)
        {
            string rootFolder = relativeTexturePath.Split('/')[0];
            string rootDirectory = Regex.Split(source.originalDirectory, rootFolder)[0];
            SourceFile materialSource = new SourceFile(
                Path.GetFileNameWithoutExtension(relativeTexturePath),
                Path.Combine(rootDirectory, relativeTexturePath),
                source.folderNameInProject,
                prefabExtension: Path.GetExtension(relativeTexturePath));
            if (!File.Exists(materialSource.originalPath))
            {
                Debug.Log("Warning! Texture not found: " + materialSource.originalPath);
                return;
            }
            materialSource.CopyToPrefabsDirectory();
            // Add the texture.
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(materialSource.prefabPathFromAssets);
            material.SetTexture("_MainTex", texture);
        }
    }
}