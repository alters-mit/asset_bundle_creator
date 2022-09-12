using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A URDF link element.
    /// </summary>
    public struct Link
    {
        /// <summary>
        /// The default mass.
        /// </summary>
        private const float DEFAULT_MASS = 1;


        /// <summary>
        /// The name of the node.
        /// </summary>
        public string name;
        /// <summary>
        /// The visual data.
        /// </summary>
        public Visual[] visuals;
        /// <summary>
        /// The pose data. Can be null.
        /// </summary>
        public Pose? pose;
        /// <summary>
        /// The inertial data.
        /// </summary>
        public Inertial inertial;


        public Link(XElement element, string sourceDirectory, string folderNameInProject, CoordinateSpace coordinateSpace, float globalScale)
        {
            name = element.GetAttributeValue("name", "link");
            // Get all of the visual elements.
            visuals = element.Elements("visual").Select(e => new Visual(e, sourceDirectory, folderNameInProject, coordinateSpace, globalScale)).ToArray();
            // If there are no visuals, search for collisions.
            if (visuals.Length == 0)
            {
                visuals = element.Elements("collision").Select(e => new Visual(e, sourceDirectory, folderNameInProject, coordinateSpace, globalScale)).ToArray();
                if (visuals.Length == 0)
                {
                    Debug.Log("Didn't find any visual elements.");
                }
                else
                {
                    Debug.Log("Didn't find any visual elements. Using collision elements instead.");
                }
            }
            // Get the pose.
            XElement poseElement = element.Element("pose");
            if (poseElement == null)
            {
                pose = null;
            }
            else
            {
                pose = Pose.FromPoseElement(poseElement, coordinateSpace);
            }
            // Set the inertial values.
            inertial = new Inertial(element.Element("inertial"), coordinateSpace);
        }


        /// <summary>
        /// Add visual mesh objects and hull colliders to the parent GameObject.
        /// </summary>
        /// <param name="go">The parent GameObject.</param>
        /// <param name="materials">A dictionary of top-level materials. Key = The name of the material.</param>
        /// <param name="meshRotation">A quaternion for correcting the rotation.</param>
        public bool AddVisualsAndColliders(ref GameObject go, SourceFile source, Dictionary<string, UrdfMaterial> materials, Quaternion meshRotation)
        {
            GameObject visualsParent = new GameObject("visuals");
            visualsParent.ParentAtZero(go);
            GameObject collidersParent = new GameObject("Generated Colliders");
            collidersParent.ParentAtZero(go);
            foreach (Visual visual in visuals)
            {
                // Add visuals.
                GameObject[] vs = visual.geometry.GetVisuals();
                foreach (GameObject v in vs)
                {
                    SetChildTransform(visualsParent, v, visual, meshRotation);
                }
                // Apply a material.
                if (visual.material != null)
                {
                    Material mat;
                    if (!visual.material.CreateMaterial(source, materials, out mat))
                    {
                        Debug.LogError("Failed to create material for: " + visual.name);
                        return false;
                    }
                    foreach (GameObject v in vs)
                    {
                        v.GetComponent<MeshRenderer>().sharedMaterial = mat;
                    }
                }
                // Get collider meshes.
                GameObject[] cs;
                if (!visual.geometry.GetColliders(out cs))
                {
                    return false;
                }
                foreach (GameObject c in cs)
                {
                    SetChildTransform(collidersParent, c, visual, meshRotation);
                }
            }
            return true;
        }


        /// <summary>
        /// Set the transform of a visual or collider child object.
        /// </summary>
        /// <param name="parent">The empty parent object.</param>
        /// <param name="child">The child object.</param>
        /// <param name="visual">The visual.</param>
        /// <param name="meshRotation">The mesh rotation.</param>
        private void SetChildTransform(GameObject parent, GameObject child, Visual visual, Quaternion meshRotation)
        {
            // Parent the child to the parent empty object.
            child.transform.parent = parent.transform;
            // Set the local position.
            child.transform.localPosition = visual.pose.position;
            // Set the local rotation.
            child.transform.localEulerAngles = visual.pose.eulerAngles;
            // Correct the local rotation for meshes.
            if (visual.geometry is MeshGeometry)
            {
                child.transform.localRotation *= meshRotation;
            }
        }
    }
}
