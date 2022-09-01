using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// A geometry that uses a primtive shape e.g. a cube.
    /// </summary>
    /// <typeparam name="T">The type of collider.</typeparam>
    public abstract class PrimitiveGeometry<T> : Geometry
        where T: Collider
    {
        public PrimitiveGeometry(XElement element, float globalScale) : base(element, globalScale)
        {

        }


        public sealed override bool GetColliders(out GameObject[] colliders)
        {
            // Create the child collider object.
            GameObject colliderObject = new GameObject();
            colliderObject.name = "collider";
            T collider = colliderObject.AddComponent<T>();
            SetCollider(ref collider);
            colliders = new GameObject[] { collider.gameObject };
            return true;
        }


        public sealed override GameObject[] GetVisuals()
        {
            // Create a primitive.
            GameObject go = GameObject.CreatePrimitive(GetPrimitiveType());
            go.transform.localScale = GetVisualScale();
            return new GameObject[] { go };
        }


        /// <summary>
        /// Returns the scale for the visual object.
        /// </summary>
        protected abstract Vector3 GetVisualScale();


        /// <summary>
        /// Returns the geometry shape.
        /// </summary>
        protected abstract PrimitiveType GetPrimitiveType();


        /// <summary>
        /// Set the collider.
        /// </summary>
        /// <param name="collider">The collider.</param>
        protected abstract void SetCollider(ref T collider);
    }
}