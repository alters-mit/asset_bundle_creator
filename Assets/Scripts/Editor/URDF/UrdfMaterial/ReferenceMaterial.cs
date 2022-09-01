using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace URDF
{
    /// <summary>
    /// A material element that references another material.
    /// </summary>
    public class ReferenceMaterial : UrdfMaterial
    {
        public ReferenceMaterial(XElement element) : base(element)
        {
        }


        public override bool CreateMaterial(SourceFile source, Dictionary<string, UrdfMaterial> materials, out Material material)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(GetMaterialPath(source.folderNameInProject));
            // Use an existing material.
            if (m != null)
            {
                material = m;
                return true;
            }
            // Try to find the referenced material.
            else if (materials.ContainsKey(name))
            {
                Debug.Log("Found referenced material: " + name);
                return materials[name].CreateMaterial(source, materials, out material);
            }
            else
            {
                Debug.LogError("Error! No reference material for: " + name);
                material = default;
                return false;
            }
        }


        protected override void SetMaterial(SourceFile source, ref Material material)
        {
        }
    }
}