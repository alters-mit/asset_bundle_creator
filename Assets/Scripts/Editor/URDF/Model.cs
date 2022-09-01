using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using SubalternGames;


namespace URDF
{
    /// <summary>
    /// URDF data for a model.
    /// </summary>
    public struct Model
    {
        /// <summary>
        /// The name of the model.
        /// </summary>
        public string name;
        /// <summary>
        /// If true, this is a static model.
        /// </summary>
        public bool isStatic;
        /// <summary>
        /// The model pose.
        /// </summary>
        public Pose pose;
        /// <summary>
        /// The model's joints.
        /// </summary>
        public List<UrdfJoint> joints;
        /// <summary>
        /// The model's links (objects and sub-objects).
        /// </summary>
        public List<Link> links;
        /// <summary>
        /// A list of model-level materials. These can be referenced elsewhere.
        /// </summary>
        public Dictionary<string, UrdfMaterial> materials;
        /// <summary>
        /// The rotation for the visuals and collisions.
        /// </summary>
        public Quaternion meshRotation;
        /// <summary>
        /// The mesh rotation in ROS coordinate space.
        /// https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/Extensions/UrdfRobotExtensions.cs
        /// </summary>
        [JsonIgnore]
        private readonly static Quaternion RosMeshRotation = Quaternion.Euler(-90, 0, 90);
        /// <summary>
        /// The mesh rotation in PartNet Mobility coordinate space.
        /// </summary>
        [JsonIgnore]
        private readonly static Quaternion PartNetMobilityRotation = Quaternion.Euler(0, 180, 0);


        public Model(string name, XElement element, string sourceDirectory, CoordinateSpace coordinateSpace, float globalScale)
        {
            this.name = name;
            string folderNameInProject = name;
            XElement staticElement = element.Element("static");
            if (staticElement == null)
            {
                isStatic = true;
            }
            else
            {
                isStatic = staticElement.Value.ToLower() == "true";
            }
            pose = Pose.FromPoseElement(element.Element("pose"), coordinateSpace);
            // Get the top-level materials.
            materials = new Dictionary<string, UrdfMaterial>();
            IEnumerable<XElement> materialElements = element.Elements("material");
            foreach (XElement materialElement in materialElements)
            {
                UrdfMaterial material = UrdfMaterial.Get(materialElement);
                materials.Add(material.name, material);
            }
            // Get the joints.
            joints = new List<UrdfJoint>();
            IEnumerable<XElement> jointElements = element.Elements("joint");
            foreach (XElement jointElement in jointElements)
            {
                // Get the joint type.
                string jointType = jointElement.Attribute("type").Value.ToLower();
                UrdfJoint joint;
                if (jointType == "fixed")
                {
                    joint = new UrdfFixedJoint(jointElement, coordinateSpace);
                }
                else if (jointType == "continuous")
                {
                    joint = new UrdfContinuousJoint(jointElement, coordinateSpace);
                }
                else if (jointType == "revolute")
                {
                    joint = new UrdfRevoluteJoint(jointElement, coordinateSpace);
                }
                else if (jointType == "prismatic")
                {
                    joint = new UrdfPrismaticJoint(jointElement, coordinateSpace);
                }
                else
                {
                    throw new System.Exception(jointType);
                }
                joints.Add(joint);
            }
            // Get the links.
            links = new List<Link>();
            IEnumerable<XElement> linkElements = element.Elements("link");
            foreach (XElement linkElement in linkElements)
            {
                links.Add(new Link(linkElement, sourceDirectory, folderNameInProject, coordinateSpace, globalScale));
            }
            // Set the mesh rotation.
            if (coordinateSpace == CoordinateSpace.unity)
            {
                meshRotation = Quaternion.identity;
            }
            else if (coordinateSpace == CoordinateSpace.partnet_mobility)
            {
                meshRotation = PartNetMobilityRotation;
            }
            else if (coordinateSpace == CoordinateSpace.ros)
            {
                meshRotation = RosMeshRotation;
            }
            else
            {
                throw new System.Exception(coordinateSpace.ToString());
            }
        }


        /// <summary>
        /// Write the a JSON serialization representation of this object to disk.
        /// </summary>
        /// <param name="outputDirectory"></param>
        public void WriteToDisk(string outputDirectory)
        {
            string path = Path.Combine(outputDirectory, "model.json").FixWindowsPath();
            JsonWrapper.Serialize(this, path, true);
            Debug.Log("Serialized model: " + path);
        }
    }
}