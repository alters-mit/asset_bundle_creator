using System.Xml.Linq;
using UnityEngine;
using Unity.Robotics.UrdfImporter;


namespace URDF
{
    public class Inertial
    {
        /// <summary>
        /// The default mass.
        /// </summary>
        private const float DEFAULT_MASS = 0.1f;
        /// <summary>
        /// The minimum inertia.
        /// </summary>
        private const float MIN_INERTIA = 1e-6f;


        /// <summary>
        /// The mass.
        /// </summary>
        public float mass = DEFAULT_MASS;
        /// <summary>
        /// The center of mass.
        /// </summary>
        public Vector3 centerOfMass;
        /// <summary>
        /// The inertia tensor.
        /// </summary>
        public Vector3 inertiaTensor;
        /// <summary>
        /// The inertia tensor rotation.
        /// </summary>
        public Quaternion inertiaTensorRotation;
        /// <summary>
        /// The default inertia tensor.
        /// Source: http://wiki.ros.org/urdf/Tutorials/Adding%20Physical%20and%20Collision%20Properties%20to%20a%20URDF%20Model
        /// </summary>
        private readonly static float[] DefaultInertiaTensor = new float[] { 1e-3f, 0, 0, 1e-3f, 0, 1e-3f };


        public Inertial(XElement element, CoordinateSpace coordinateSpace)
        {
            // Set default values.
            if (element == null)
            {
                mass = DEFAULT_MASS;
                centerOfMass = Vector3.zero;
                SetInertiaTensor(DefaultInertiaTensor, Vector3.zero, coordinateSpace);
                return;
            }
            XElement massElement = element.Element("mass");
            if (massElement == null)
            {
                mass = DEFAULT_MASS;
            }
            else
            {
                XAttribute massValueAttribute = massElement.Attribute("value");
                if (massValueAttribute != null)
                {
                    mass = float.Parse(massValueAttribute.Value);
                }
                else
                {
                    mass = DEFAULT_MASS;
                }
            }
            Pose pose = Pose.FromOriginElement(element, coordinateSpace);
            centerOfMass = pose.position;
            float[] inertiaTensorValues;
            XElement inertiaElement = element.Element("inertia");
            if (inertiaElement == null)
            {
                inertiaTensorValues = DefaultInertiaTensor;
            }
            else
            {
                // Parse the interia data.
                inertiaTensorValues = new float[] {
                    float.Parse(inertiaElement.Attribute("ixx").Value),
                    float.Parse(inertiaElement.Attribute("ixy").Value),
                    float.Parse(inertiaElement.Attribute("ixz").Value),
                    float.Parse(inertiaElement.Attribute("iyy").Value),
                    float.Parse(inertiaElement.Attribute("iyz").Value),
                    float.Parse(inertiaElement.Attribute("izz").Value)
                };
            }
            // Set the inertia tensor properties.
            SetInertiaTensor(inertiaTensorValues, pose.eulerAngles, coordinateSpace);
        }


        /// <summary>
        /// Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/UrdfComponents/UrdfInertial.cs
        /// </summary>
        /// <param name="inertiaTensorValues">The inertia tensor values.</param>
        /// <param name="coordinateSpace">The coordinate space.</param>
        /// <param name="eulerAngles">The inertia Euler angles.</param>
        private void SetInertiaTensor(float[] inertiaTensorValues, Vector3 eulerAngles, CoordinateSpace coordinateSpace)
        {
            Vector3 eigenvalues;
            Vector3[] eigenvectors;
            Matrix3x3 rotationMatrix = new Matrix3x3(inertiaTensorValues);
            rotationMatrix.DiagonalizeRealSymmetric(out eigenvalues, out eigenvectors);
            inertiaTensor = ToUnityInertiaTensor(FixMinInertia(eigenvalues));
            inertiaTensorRotation = ToQuaternion(eigenvectors[0], eigenvectors[1], eigenvectors[2]).InCoordinateSpace(coordinateSpace) * Quaternion.Euler(eulerAngles);
        }


        /// <summary>
        /// Convert the eigen values to a Unity inertia tensor.
        /// </summary>
        /// <param name="vector3">The eigenvalues vector.</param>
        private static Vector3 ToUnityInertiaTensor(Vector3 vector3)
        {
            return new Vector3(vector3.y, vector3.z, vector3.x);
        }



        /// <summary>
        /// Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/UrdfComponents/UrdfInertial.cs
        /// </summary>
        /// <param name="vector3">The eigenvalues vector.</param>
        private static Vector3 FixMinInertia(Vector3 vector3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (vector3[i] < MIN_INERTIA)
                    vector3[i] = MIN_INERTIA;
            }
            return vector3;
        }


        /// <summary>
        /// Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/UrdfComponents/UrdfInertial.cs
        /// </summary>
        private static Quaternion ToQuaternion(Vector3 eigenvector0, Vector3 eigenvector1, Vector3 eigenvector2)
        {
            //From http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
            float tr = eigenvector0[0] + eigenvector1[1] + eigenvector2[2];
            float qw, qx, qy, qz;
            if (tr > 0)
            {
                float s = Mathf.Sqrt(tr + 1.0f) * 2f; // S=4*qw 
                qw = 0.25f * s;
                qx = (eigenvector1[2] - eigenvector2[1]) / s;
                qy = (eigenvector2[0] - eigenvector0[2]) / s;
                qz = (eigenvector0[1] - eigenvector1[0]) / s;
            }
            else if ((eigenvector0[0] > eigenvector1[1]) & (eigenvector0[0] > eigenvector2[2]))
            {
                float s = Mathf.Sqrt(1.0f + eigenvector0[0] - eigenvector1[1] - eigenvector2[2]) * 2; // S=4*qx 
                qw = (eigenvector1[2] - eigenvector2[1]) / s;
                qx = 0.25f * s;
                qy = (eigenvector1[0] + eigenvector0[1]) / s;
                qz = (eigenvector2[0] + eigenvector0[2]) / s;
            }
            else if (eigenvector1[1] > eigenvector2[2])
            {
                float s = Mathf.Sqrt(1.0f + eigenvector1[1] - eigenvector0[0] - eigenvector2[2]) * 2; // S=4*qy
                qw = (eigenvector2[0] - eigenvector0[2]) / s;
                qx = (eigenvector1[0] + eigenvector0[1]) / s;
                qy = 0.25f * s;
                qz = (eigenvector2[1] + eigenvector1[2]) / s;
            }
            else
            {
                float s = Mathf.Sqrt(1.0f + eigenvector2[2] - eigenvector0[0] - eigenvector1[1]) * 2; // S=4*qz
                qw = (eigenvector0[1] - eigenvector1[0]) / s;
                qx = (eigenvector2[0] + eigenvector0[2]) / s;
                qy = (eigenvector2[1] + eigenvector1[2]) / s;
                qz = 0.25f * s;
            }
            return new Quaternion(qx, qy, qz, qw);
        }
    }
}