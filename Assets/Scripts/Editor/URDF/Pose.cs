using System.Xml.Linq;
using UnityEngine;


namespace URDF
{
    /// <summary>
    /// The position and Euler angles of a URDF link.
    /// </summary>
    public struct Pose
    {
        /// <summary>
        /// The position of the model.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The anchor position of the model's joint.
        /// </summary>
        public Vector3 anchorPosition;
        /// <summary>
        /// The rotational Euler angles of the model.
        /// </summary>
        public Vector3 eulerAngles;


        public Pose(Vector3 position, Vector3 anchorPosition, Vector3 eulerAngles)
        {
            this.position = position;
            this.anchorPosition = anchorPosition;
            this.eulerAngles = eulerAngles;
        }


        /// <summary>
        /// Returns a Pose from a pose element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="coordinateSpace">The coordinate space.</param>
        public static Pose FromPoseElement(XElement element, CoordinateSpace coordinateSpace)
        {
            if (element == null)
            {
                return default;
            }
            float[] pose = element.Value.ToArray();
            Vector3 position = new Vector3(pose[0], pose[1], pose[2]);
            return new Pose(position.PositionInCoordinateSpace(coordinateSpace), 
                position.AnchorInCoordinateSpace(coordinateSpace),
                new Vector3(pose[3], pose[4], pose[5]).EulerAnglesInCoordinateSpace(coordinateSpace));
        }


        /// <summary>
        /// Returns a Pose from an origin element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="coordinateSpace">The coordinate space.</param>
        public static Pose FromOriginElement(XElement element, CoordinateSpace coordinateSpace)
        {
            // If there is no origin, assume that everything is (0, 0, 0).
            if (element == null)
            {
                return default;
            }
            // Parse the origin element to get the pose.
            else
            {
                Vector3 position;
                if (!element.GetPositionAttribute(coordinateSpace, out position))
                {
                    position = Vector3.zero;
                }
                Vector3 anchorPosition;
                if (!element.GetAnchorPositionAttribute(coordinateSpace, out anchorPosition))
                {
                    anchorPosition = Vector3.zero;
                }
                Vector3 eulerAngles;
                if (!element.GetEulerAnglesAttribute(coordinateSpace, out eulerAngles))
                {
                    eulerAngles = Vector3.zero;
                }
                return new Pose(position, anchorPosition, eulerAngles);
            }
        }
    }
}