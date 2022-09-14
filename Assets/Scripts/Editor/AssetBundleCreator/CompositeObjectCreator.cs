using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using URDF;
using Logger = Logging.Logger;
using Object = UnityEngine.Object;
using Pose = URDF.Pose;


/// <summary>
/// Create a composite object from URDF data.
/// This is separate from the RobotCreator because it will use Joints instead of ArticulationBodies. Joints are buggier but allow the object to be manipulated in more ways e.g. by a VR agent.
/// </summary>
public class CompositeObjectCreator : AssetBundleCreator<CompositeObjectCreator, ModelRecord>
{
    /// <summary>
    /// If true, the robot is immovable.
    /// </summary>
    private readonly bool immovable;
    /// <summary>
    /// The VHACD resolution.
    /// </summary>
    private readonly int vhacdResolution;
    /// <summary>
    /// The WordNet ID.
    /// </summary>
    private readonly string wnid;
    /// <summary>
    /// The WordNet category.
    /// </summary>
    private readonly string wcategory;
    /// <summary>
    /// The root XML node.
    /// </summary>
    private readonly XElement rootNode;
    /// <summary>
    /// If we're going to override the mesh scale, use this value.
    /// </summary>
    private readonly float globalScale;
    /// <summary>
    /// The coordinate space.
    /// </summary>
    private readonly CoordinateSpace coordinateSpace;


    public CompositeObjectCreator(string name, string source, 
        string outputDirectory, int vhacdResolution = Constants.DEFAULT_VHACD_RESOLUTION, string wnid = "", string wcategory = "", float globalScale = 1) :
        base(name, source, outputDirectory)
    {
        this.vhacdResolution = vhacdResolution;
        this.wnid = wnid;
        this.wcategory = wcategory;
        this.globalScale = globalScale;
        // Load the XML from the file.
        XDocument doc = XDocument.Load(source);
        rootNode = doc.Root;
        coordinateSpace = GetCoordinateSpace();
        immovable = IsImmovable();
    }


    public CompositeObjectCreator(XElement rootElement, string source,
        string outputDirectory, int vhacdResolution = Constants.DEFAULT_VHACD_RESOLUTION, string wnid = "", string wcategory = "", float globalScale = 1) :
        base(rootElement.Attribute("name").Value, source, outputDirectory)
    {
        this.vhacdResolution = vhacdResolution;
        this.wnid = wnid;
        this.wcategory = wcategory;
        rootNode = rootElement;
        coordinateSpace = GetCoordinateSpace();
        this.globalScale = globalScale;
        immovable = IsImmovable();
    }


    public CompositeObjectCreator() : base()
    {
        vhacdResolution = ArgumentParser.TryGet("vhacd_resolution", Constants.DEFAULT_VHACD_RESOLUTION);
        wnid = ArgumentParser.TryGet("wnid", "");
        wcategory = ArgumentParser.TryGet("wcategory", "");
        globalScale = 1;
        // Load the XML from the file.
        XDocument doc = XDocument.Load(source.originalPath);
        rootNode = doc.Root;
        coordinateSpace = GetCoordinateSpace();
        immovable = IsImmovable();
    }


    /// <summary>
    /// Returns the model.
    /// </summary>
    /// <returns></returns>
    public Model GetModel()
    {
        return new Model(name, rootNode, source.originalDirectory, coordinateSpace, globalScale);
    }


    public override bool CreatePrefab()
    {
        Logger.StartLogging(logPath);
        Debug.Log("Creating prefab for: " + name);
        Model model = GetModel();
        model.WriteToDisk(outputDirectory);
        // Create a new game object.
        GameObject go = new GameObject(model.name);
        Dictionary<string, GameObject> linkObjects = new Dictionary<string, GameObject>();
        Dictionary<string, Link> links = new Dictionary<string, Link>();
        foreach (Link link in model.links)
        {
            GameObject linkObject = new GameObject(link.name);
            Debug.Log("Created link: " + link.name);
            // Add visuals and colliders.
            if (!link.AddVisualsAndColliders(ref linkObject, source, model.materials, model.visualMeshRotation, model.colliderMeshRotation))
            {
                Debug.Log("Error adding visuals or colliders.");
                return false;
            }
            // Remember the link.
            linkObjects.Add(link.name, linkObject);
            links.Add(link.name, link);
        }
        // Parent each link.
        foreach (string linkName in linkObjects.Keys)
        {
            foreach (UrdfJoint joint in model.joints)
            {
                if (joint.child == linkName)
                {
                    foreach (string parentLinkName in linkObjects.Keys)
                    {
                        if (joint.parent == parentLinkName)
                        {
                            // Parent the object.
                            linkObjects[linkName].transform.parent = linkObjects[parentLinkName].transform;
                            Debug.Log("Parented " + linkName + " to " + parentLinkName);
                            // Use the link pose.
                            if (links[linkName].pose != null)
                            {
                                Pose pose = (Pose)(links[linkName].pose);
                                linkObjects[linkName].transform.localPosition = pose.position;
                                linkObjects[linkName].transform.localEulerAngles = pose.eulerAngles;
                            }
                            // Use the joint pose.
                            else
                            {
                                linkObjects[linkName].transform.localPosition = joint.pose.position;
                                linkObjects[linkName].transform.localEulerAngles = joint.pose.eulerAngles;
                            }
                            if (joint is UrdfFixedJoint)
                            {
                                Debug.Log(linkName + " is a fixed joint. No joint component has been added.");
                            }
                            // Create a hinge joint.
                            else if (joint is UrdfRevoluteJoint || joint is UrdfContinuousJoint)
                            {
                                // Create the component.
                                HingeJoint hingeJoint = linkObjects[linkName].AddComponent<HingeJoint>();
                                hingeJoint.connectedBody = linkObjects[parentLinkName].GetOrAddComponent<Rigidbody>();
                                // Set the anchor and axis.
                                UrdfContinuousJoint ucj = (UrdfContinuousJoint)joint;
                                hingeJoint.anchor = ucj.pose.anchorPosition;
                                hingeJoint.autoConfigureConnectedAnchor = false;
                                hingeJoint.connectedAnchor = ucj.pose.anchorPosition;
                                hingeJoint.axis = ucj.axis;
                                Debug.Log("Added HingeJoint component to " + linkName);
                                Debug.Log("Set connected body: " + parentLinkName);
                                // Set the limits.
                                if (joint is UrdfRevoluteJoint)
                                {
                                    UrdfRevoluteJoint urj = (UrdfRevoluteJoint)joint;
                                    hingeJoint.useLimits = true;
                                    hingeJoint.limits = new JointLimits()
                                    {
                                        min = urj.limitLower,
                                        max = urj.limitUpper
                                    };
                                }
                                else
                                {
                                    hingeJoint.useLimits = false;
                                }
                                // Set the inertial properties.
                            }
                            // Create a prismatic joint.
                            // Source: https://github.com/Unity-Technologies/URDF-Importer/blob/main/com.unity.robotics.urdf-importer/Runtime/UrdfComponents/UrdfJoints/UrdfJointPrismatic.cs
                            else if (joint is UrdfPrismaticJoint)
                            {
                                // Create the component.
                                ConfigurableJoint configurableJoint = linkObjects[linkName].AddComponent<ConfigurableJoint>();
                                configurableJoint.connectedBody = linkObjects[parentLinkName].GetOrAddComponent<Rigidbody>();
                                UrdfPrismaticJoint upc = (UrdfPrismaticJoint)joint;
                                // Set the anchor and axis.
                                configurableJoint.anchor = new Vector3(upc.pose.anchorPosition.x,
                                    upc.pose.anchorPosition.y + upc.limitLower + upc.limitUpper,
                                    upc.pose.anchorPosition.z);
                                configurableJoint.axis = upc.axis;
                                configurableJoint.secondaryAxis = upc.axis;
                                // Set the motion limits.
                                configurableJoint.xMotion = ConfigurableJointMotion.Limited;
                                configurableJoint.yMotion = ConfigurableJointMotion.Locked;
                                configurableJoint.zMotion = ConfigurableJointMotion.Locked;
                                configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
                                configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
                                configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
                                SoftJointLimit limit = configurableJoint.linearLimit;
                                limit.limit = upc.limitLower;
                                configurableJoint.linearLimit = limit;
                                Debug.Log("Added ConfigurableJoint component to " + linkName);
                                Debug.Log("Set connected body: " + parentLinkName);
                            }
                            // Set the inertial properties.
                            Rigidbody jointRigidbody = linkObjects[linkName].GetComponent<Rigidbody>();
                            if (jointRigidbody != null)
                            {
                                jointRigidbody.mass = links[linkName].inertial.mass;
                                jointRigidbody.centerOfMass = links[linkName].inertial.centerOfMass;
                                jointRigidbody.inertiaTensor = links[linkName].inertial.inertiaTensor;
                                jointRigidbody.inertiaTensorRotation = links[linkName].inertial.inertiaTensorRotation;
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }
        // Parent any remaining links to the root object.
        foreach (string linkName in linkObjects.Keys)
        {
            if (linkObjects[linkName].transform.parent == null)
            {
                linkObjects[linkName].transform.parent = go.transform;
                Debug.Log("Parented " + linkName + " to " + go.name);
            }
        }
        // Add a Rigidbody.
        Rigidbody r = go.AddComponent<Rigidbody>();
        if (model.isStatic)
        {
            r.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        r.isKinematic = model.isStatic;
        // Get rigidbodies. For each rigidbody that doesn't have a joint, add a Fixedjoint.
        Rigidbody[] rigidbodies = go.GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            // Ignore the root object.
            if (rigidbodies[i].Equals(r))
            {
                continue;
            }
            if (rigidbodies[i].GetComponent<Joint>() == null)
            {
                rigidbodies[i].gameObject.AddComponent<FixedJoint>();
                Debug.Log("Added a FixedJoint to Rigidbody: " + rigidbodies[i]);
            }
        }
        // Set connected bodies.
        Joint[] joints = go.GetComponentsInChildren<Joint>();
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].gameObject.Equals(go) || joints[i].connectedBody != null)
            {
                continue;
            }
            Transform jointParent = joints[i].transform.parent;
            bool doneCheckingParents = false;
            while (!doneCheckingParents)
            {
                Rigidbody rigidbodyParent = jointParent.gameObject.GetComponent<Rigidbody>();
                // This parent object has a Rigidbody. Use it as the connected body.
                if (rigidbodyParent != null)
                {
                    joints[i].connectedBody = rigidbodyParent;
                    Debug.Log("Set " + joints[i].name + " connected body: " + rigidbodyParent.name);
                    doneCheckingParents = true;
                }
                else
                {
                    // Get the next ancestor.
                    jointParent = jointParent.parent;
                    // There are no more parents.
                    if (jointParent == null)
                    {
                        doneCheckingParents = true;
                    }
                }
            }
            if (doneCheckingParents && joints[i].connectedBody == null)
            {
                Debug.Log("Error! Failed to get joint connected body for: " + joints[i].name);
                return false;
            }
        }
        // Create the prefab. 
        GameObjectToPrefab(go);
        Debug.Log("Created prefab!");
        Logger.StopLogging();
        return true;
    }


    protected override ModelRecord GetRecord()
    {
        // Instantiate the prefab.
        GameObject go = source.InstianateFromPrefab();
        // Get the volume.
        float volume = 0;
        Collider[] colliders = go.GetComponentsInChildren<Collider>();
        // Add the volume of each collider hull.
        for (int i = 0; i < colliders.Length; i++)
        {
            RotatedBounds b = new RotatedBounds(colliders[i]);
            volume += Vector3.Distance(b.front, b.back) *
                Vector3.Distance(b.left, b.right) *
                Vector3.Distance(b.top, b.bottom);
        }
        // Append Flex data.
        ModelRecord record = new ModelRecord()
        {
            name = name,
            urls = GetURLs(),
            wnid = wnid,
            wcategory = wcategory,
            scale_factor = 1,
            do_not_use = false,
            do_not_use_reason = "",
            canonical_rotation = Vector3.zero,
            flex = false,
            asset_bundle_sizes = new AssetBundleSizes()
            {
                Darwin = GetAssetBundleFileSize(BuildTarget.StandaloneOSX),
                Linux = GetAssetBundleFileSize(BuildTarget.StandaloneLinux64),
                Windows = GetAssetBundleFileSize(BuildTarget.StandaloneWindows64)
            },
            physics_quality = 1,
            bounds = new RotatedBounds(go),
            substructure = SubObject.GetSubstructure(go),
            composite_object = true,
            volume = volume,
            container_shapes = new object[0]
        };
        // Destroy the object.
        Object.DestroyImmediate(go);
        return record;
    }


    /// <summary>
    /// Returns true if a static node is present and the value is true.
    /// </summary>
    private bool IsImmovable()
    {
        XElement staticElement = rootNode.Element("static");
        if (staticElement == null)
        {
            return true;
        }
        else
        {
            return staticElement.Value.ToLower() == "true";
        }
    }


    /// <summary>
    /// Returns true if this is a PartNet Mobility model.
    /// </summary>
    /// <returns></returns>
    private bool IsPartNetMobility()
    {
        return rootNode.Name == "robot" && rootNode.Attribute("name").Value.StartsWith("partnet_");
    }


    /// <summary>
    /// Returns the coordinate space by deciding whether this is a PartNet Mobility .urdf file.
    /// </summary>
    private CoordinateSpace GetCoordinateSpace()
    {
        Logger.StartLogging(logPath);
        if (IsPartNetMobility())
        {
            Debug.Log("This is a PartNet Mobility model.");
            Logger.StopLogging();
            return CoordinateSpace.partnet_mobility;
        }
        else
        {
            Debug.Log("This is model is in the ROS coordinate space.");
            Logger.StopLogging();
            return CoordinateSpace.ros;
        }
    }
}