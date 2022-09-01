using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace LISDF
{
    /// <summary>
    /// Metadata for an asset bundle.
    /// </summary>
    public struct LisdfAssetBundle
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        public string name;
        /// <summary>
        /// If true, this is a model. If false, this is a robot.
        /// </summary>
        public bool isModel;
        /// <summary>
        /// If true, this is a composite object.
        /// </summary>
        public bool isCompositeObject;
        /// <summary>
        /// A list of joint names.
        /// </summary>
        public string[] jointNames;
        /// <summary>
        /// The axis of rotation per HingeJoint.
        /// </summary>
        public Dictionary<string, Vector3> hingeJointAxes;



        public LisdfAssetBundle(string path)
        {
            // Load the asset bundle.
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            GameObject go = assetBundle.LoadAllAssets()[0] as GameObject;
            name = go.name;
            // If there are Rigidbodies, this is a composite object. Otherwise, it's a robot.
            isModel = go.GetComponentsInChildren<Rigidbody>().Length > 0;
            hingeJointAxes = new Dictionary<string, Vector3>();
            if (isModel)
            {
                jointNames = go.GetComponentsInChildren<Joint>().Select(joint => joint.name).ToArray();
                HingeJoint[] hinges = go.GetComponentsInChildren<HingeJoint>();
                foreach (HingeJoint hinge in hinges)
                {
                    hingeJointAxes.Add(hinge.name, hinge.axis);
                }
                isCompositeObject = go.GetComponentsInChildren<Rigidbody>().Length > 1;
            }
            else
            {
                isCompositeObject = false;
                jointNames = go.GetComponentsInChildren<ArticulationBody>().Select(joint => joint.name).ToArray();
            }
            // Unload the asset bundle.
            assetBundle.Unload(true);
        }
    }
}