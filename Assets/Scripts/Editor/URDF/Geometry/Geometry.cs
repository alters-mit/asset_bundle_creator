using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// URDF geometry data.
    /// </summary>
    public abstract class Geometry
    {
        /// <summary>
        /// The global mesh scale.
        /// </summary>
        public float globalScale;


        public Geometry(XElement element, float globalScale)
        {
            this.globalScale = globalScale;
        }


        /// <summary>
        /// Returns a Geometry sub-class.
        /// </summary>
        /// <param name="element">The root element of the geometry.</param>
        /// <param name="sourceDirectory">The root source directory.</param>
        /// <param name="folderNameInProject">The folder name in prefabs and source_files.</param>
        /// <param name="coordinateSpace">The coordinate space.</param>
        /// <param name="globalScale">The global mesh scale.</param>
        public static Geometry Get(XElement element, string sourceDirectory, string folderNameInProject, CoordinateSpace coordinateSpace, float globalScale)
        {
            XElement boxElement = element.Element("box");
            XElement cylinderElement = element.Element("cylinder");
            XElement sphereElement = element.Element("sphere");
            XElement meshElement = element.Element("mesh");
            if (boxElement != null)
            {
                return new BoxGeometry(boxElement, coordinateSpace, globalScale);
            }
            else if (cylinderElement != null)
            {
                return new CylinderGeometry(cylinderElement, globalScale);
            }
            else if (sphereElement != null)
            {

                return new SphereGeometry(sphereElement, globalScale);
            }
            else if (meshElement != null)
            {
                return new MeshGeometry(folderNameInProject, meshElement, sourceDirectory, globalScale);
            }
            else
            {
                throw new System.Exception(element.Value);
            }
        }


        /// <summary>
        /// Returns an array of visual objects.
        /// </summary>
        public abstract GameObject[] GetVisuals();


        /// <summary>
        /// Returns an array of hull collider objects.
        /// </summary>
        /// <param name="colliders">The colliders.</param>
        public abstract bool GetColliders(out GameObject[] colliders);
    }
}