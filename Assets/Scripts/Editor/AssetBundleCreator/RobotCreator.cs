using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TDW.Robotics;
using URDF;
using Logger = Logging.Logger;


/// <summary>
/// Create a model asset bundle from a .urdf
/// </summary>
public class RobotCreator : AssetBundleCreator<RobotCreator, RobotRecord>
{
    /// <summary>
    /// The default stiffness value.
    /// </summary>
    private const float DEFAULT_STIFFNESS = 1000;
    /// <summary>
    /// The default damping value.
    /// </summary>
    private const float DEFAULT_DAMPING = 180;
    /// <summary>
    /// The robot coordinate space.
    /// </summary>
    private const CoordinateSpace COORDINATE_SPACE = CoordinateSpace.ros;


    /// <summary>
    /// If true, the robot is immovable.
    /// </summary>
    private readonly bool immovable;
    /// <summary>
    /// A description of the source of the robot file e.g. the URL.
    /// </summary>
    private readonly string sourceDescription;


    public RobotCreator(bool immovable, string sourceDescription, string name, string source, string outputDirectory) : base(name, source, outputDirectory)
    {
        this.immovable = immovable;
        this.sourceDescription = sourceDescription;
    }


    public RobotCreator(string name, string source, string outputDirectory) : base(name, source, outputDirectory)
    {
        immovable = ArgumentParser.GetBoolean("-immovable");
        sourceDescription = ArgumentParser.TryGet("source_description", "");
    }


    public RobotCreator() : base()
    {
        immovable = ArgumentParser.GetBoolean("-immovable");
        sourceDescription = ArgumentParser.TryGet("source_description", "");
    }


    /// <summary>
    /// Returns the model.
    /// </summary>
    public Model GetModel()
    {
        XDocument doc = XDocument.Load(source.originalPath);
        return new Model(name, doc.Root, source.originalDirectory, COORDINATE_SPACE, 1);
    }


    public override bool CreatePrefab()
    {
        Logger.StartLogging(logPath);
        Model robot = GetModel();
        Debug.Log("Creating prefab for: " + robot.name);
        robot.WriteToDisk(outputDirectory);
        Dictionary<string, GameObject> linkObjects = new Dictionary<string, GameObject>();
        Dictionary<string, Link> links = new Dictionary<string, Link>();
        foreach (Link link in robot.links)
        {
            GameObject linkObject = new GameObject(link.name);
            Debug.Log("Created: " + link.name);
            // Add visuals and colliders.
            if (!link.AddVisualsAndColliders(ref linkObject, source, robot.materials, robot.visualMeshRotation, robot.colliderMeshRotation))
            {
                return false;
            }
            // Remember this link so we can parent it later.
            links.Add(link.name, link);
            linkObjects.Add(link.name, linkObject);
        }
        // Create joints.
        List<ArticulationBody> articulationBodies = new List<ArticulationBody>();
        foreach (string linkName in linkObjects.Keys)
        {
            foreach (UrdfJoint joint in robot.joints)
            {
                if (joint.child == linkName)
                {
                    foreach (string parentLinkName in linkObjects.Keys)
                    {
                        if (joint.parent == parentLinkName)
                        {
                            // Get the pose.
                            URDF.Pose pose;
                            if (links[linkName].pose != null)
                            {
                                pose = (URDF.Pose)links[linkName].pose;
                            }
                            else
                            {
                                pose = joint.pose;
                            }
                            // Parent the object.
                            linkObjects[linkName].transform.parent = linkObjects[parentLinkName].transform;
                            // Set the local position and rotation.
                            linkObjects[linkName].transform.localPosition = pose.position;
                            linkObjects[linkName].transform.localEulerAngles = pose.eulerAngles;
                            // Create the ArticulationBody.
                            ArticulationBody articulationBody = linkObjects[linkName].AddComponent<ArticulationBody>();
                            // Set the anchor.
                            articulationBody.anchorPosition = pose.anchorPosition;
                            // Set the inertial properties.
                            articulationBody.mass = links[linkName].inertial.mass;
                            articulationBody.centerOfMass = links[linkName].inertial.centerOfMass;
                            articulationBody.inertiaTensor = links[linkName].inertial.inertiaTensor;
                            articulationBody.inertiaTensorRotation = links[linkName].inertial.inertiaTensorRotation;
                            Debug.Log("Adding joint " + linkName + " connected to " + parentLinkName);
                            if (joint is UrdfFixedJoint)
                            {
                                articulationBody.jointType = ArticulationJointType.FixedJoint;
                                Debug.Log("Fixed joint.");
                            }
                            else if (joint is UrdfRevoluteJoint || joint is UrdfContinuousJoint)
                            {
                                articulationBody.jointType = ArticulationJointType.RevoluteJoint;
                                // Set the drive.
                                ArticulationDrive drive = articulationBody.xDrive;
                                drive.stiffness = DEFAULT_STIFFNESS;
                                drive.damping = DEFAULT_DAMPING;
                                UrdfContinuousJoint ucj = (UrdfContinuousJoint)joint;
                                // Set the axis of rotation.
                                if (ucj.axis.x > 0)
                                {
                                    articulationBody.anchorRotation = Quaternion.Euler(90, 0, 0);
                                }
                                else if (ucj.axis.x < 0)
                                {
                                    articulationBody.anchorRotation = Quaternion.Euler(-90, 0, 0);
                                }
                                else if (ucj.axis.z > 0)
                                {
                                    articulationBody.anchorRotation = Quaternion.Euler(0, 0, 90);
                                }
                                else if (ucj.axis.z < 0)
                                {
                                    articulationBody.anchorRotation = Quaternion.Euler(0, 0, -90);
                                }
                                if (joint is UrdfRevoluteJoint)
                                {
                                    UrdfRevoluteJoint urj = (UrdfRevoluteJoint)joint;
                                    articulationBody.twistLock = ArticulationDofLock.LimitedMotion;
                                    drive.forceLimit = urj.forceLimit;
                                    drive.lowerLimit = urj.limitLower;
                                    drive.upperLimit = urj.limitUpper;
                                }
                                articulationBody.xDrive = drive;
                                Debug.Log("Revolute joint.");
                            }
                            else if (joint is UrdfPrismaticJoint)
                            {
                                UrdfPrismaticJoint upj = (UrdfPrismaticJoint)joint;
                                articulationBody.jointType = ArticulationJointType.PrismaticJoint;
                                // Get the drive from the axis.
                                ArticulationDrive drive;
                                if (upj.driveAxis == DriveAxis.x)
                                {
                                    drive = articulationBody.xDrive;
                                }
                                else if (upj.driveAxis == DriveAxis.y)
                                {
                                    drive = articulationBody.yDrive;
                                }
                                else
                                {
                                    drive = articulationBody.zDrive;
                                }
                                // Set the drive values.
                                drive.forceLimit = upj.forceLimit;
                                drive.stiffness = DEFAULT_STIFFNESS;
                                drive.damping = DEFAULT_DAMPING;
                                drive.lowerLimit = upj.limitLower;
                                drive.upperLimit = upj.limitUpper;
                                // Set the locks and assign the drive.
                                if (upj.driveAxis == DriveAxis.x)
                                {
                                    articulationBody.linearLockX = ArticulationDofLock.LimitedMotion;
                                    articulationBody.linearLockY = ArticulationDofLock.LockedMotion;
                                    articulationBody.linearLockZ = ArticulationDofLock.LockedMotion;
                                    articulationBody.xDrive = drive;
                                }
                                else if (upj.driveAxis == DriveAxis.y)
                                {
                                    articulationBody.linearLockX = ArticulationDofLock.LockedMotion;
                                    articulationBody.linearLockY = ArticulationDofLock.LimitedMotion;
                                    articulationBody.linearLockZ = ArticulationDofLock.LockedMotion;
                                    articulationBody.yDrive = drive;
                                }
                                else
                                {
                                    articulationBody.linearLockX = ArticulationDofLock.LockedMotion;
                                    articulationBody.linearLockY = ArticulationDofLock.LockedMotion;
                                    articulationBody.linearLockZ = ArticulationDofLock.LimitedMotion;
                                    articulationBody.zDrive = drive;
                                }
                                // Set the expected drive axis.
                                ExpectedDriveAxis eda = articulationBody.gameObject.AddComponent<ExpectedDriveAxis>();
                                eda.axis = upj.driveAxis;
                            }
                            else
                            {
                                Debug.LogError("Error! Joint type not supported: " + joint.GetType());
                                return false;
                            }
                            articulationBodies.Add(articulationBody);
                        }
                    }
                }
            }
        }
        // Find the root object.
        ArticulationBody root = articulationBodies.First(a => a.isRoot);
        // Set the immovability.
        root.immovable = immovable;
        Debug.Log("Found root ArticulationBody: " + root.name);
        Transform rootParent = root.transform.parent;
        // Unparent.
        root.transform.parent = null;
        // Destroy the old parent.
        if (rootParent != null)
        {
            Object.DestroyImmediate(rootParent.gameObject);
        }
        // Remove empty objects.
        ArticulationBody[] children = root.GetComponentsInChildren<ArticulationBody>();
        Debug.Log("Number of ArticulationBodies: " + children.Length);
        Debug.Log("Looking for empty ArticulationBodies...");
        for (int i = 0; i < children.Length; i++)
        {
            // Ignore the root.
            if (children[i].Equals(root))
            {
                continue;
            }
            MeshFilter[] meshFilters = children[i].GetComponentsInChildren<MeshFilter>();
            Collider[] colliders = children[i].GetComponentsInChildren<Collider>();
            ArticulationBody[] subChildren = children[i].GetComponentsInChildren<ArticulationBody>();
            if (meshFilters.Length == 0 && colliders.Length == 0 && subChildren.Length == 1)
            {
                Debug.Log("Destroyed empty ArticulationBody: " + children[i].name);
                Object.DestroyImmediate(children[i].gameObject);
            }
        }
        Debug.Log("Number of ArticulationBodies: " + root.GetComponentsInChildren<ArticulationBody>().Length);
        // Create the prefab.
        GameObjectToPrefab(root.gameObject);
        Debug.Log("Created prefab!");
        Logger.StopLogging();
        return true;
    }


    protected override RobotRecord GetRecord()
    {
        RobotRecord record = new RobotRecord();
        record.ik = new object[0];
        record.immovable = immovable;
        record.name = name;
        record.urls = GetURLs();
        record.targets = new Dictionary<object, object>();
        record.source = sourceDescription;
        return record;
    }
}
