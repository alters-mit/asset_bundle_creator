using LISDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using URDF;
using Logger = Logging.Logger;
using Pose = URDF.Pose;


/// <summary>
/// Import a .lisdf file.
/// </summary>
public static class LisdfReader
{
    /// <summary>
    /// A list of platform names.
    /// </summary>
    private readonly static string[] Platforms = new string[] { "Darwin", "Linux", "Windows" };
    /// <summary>
    /// The current output directory.
    /// </summary>
    private static string logPath;

    /// <summary>
    /// Read an .lisdf file using command line argument values for parameter values.
    /// </summary>
    public static void Read()
    {
        Read(ArgumentParser.Get("source"),
            ArgumentParser.Get("output_directory"),
            ArgumentParser.GetBoolean("-overwrite"),
            ArgumentParser.GetBoolean("-cleanup"),
            ArgumentParser.TryGet("robots", new Dictionary<string, string>()));
    }


    /// <summary>
    /// Read an .lisdf file and create asset bundles.
    /// </summary>
    /// <param name="source">The absolute file path to the .lisdf file.</param>
    /// <param name="outputDirectory">The output directory.</param>
    /// <param name="overwrite">If true, overwrite existing asset bundles.</param>
    /// <param name="cleanup">If true, cleanup intermediary files.</param>
    /// <param name="robots">A dictionary of robots. Key = Name. Value = Path to the .urdf file.</param>
    public static void Read(string source, string outputDirectory, bool overwrite, bool cleanup, Dictionary<string, string> robots)
    {
        logPath = Path.Combine(outputDirectory, Constants.LOG_FILE_NAME);
        // Remember the models so that we can map joint names to link names.
        Dictionary<string, Model> models = new Dictionary<string, Model>();
        // Start a list of command builders.
        List<CommandBuilder> commandBuilders = new List<CommandBuilder>();
        // Add the scene.
        CommandBuilder addScene = new CommandBuilder("add_scene");
        addScene.Add("name", "empty_scene");
        addScene.Add("url", "https://tdw-public.s3.amazonaws.com/scenes/" + GetAssetBundleInfix() + "/2019.2/empty_scene");
        commandBuilders.Add(addScene);
        XDocument doc = XDocument.Load(source);
        IEnumerable<XElement> cameraElements = doc.Descendants("camera");
        // Create cameras.
        foreach (XElement cameraElement in cameraElements)
        {
            LisdfCamera camera = new LisdfCamera(cameraElement);
            // Create the avatar.
            CommandBuilder createAvatar = new CommandBuilder("create_avatar");
            createAvatar.Add("type", "A_Img_Caps_Kinematic");
            createAvatar.Add("id", camera.name);
            commandBuilders.Add(createAvatar);
            // Teleport the avatar.
            CommandBuilder teleportAvatarTo = new CommandBuilder("teleport_avatar_to");
            teleportAvatarTo.Add("avatar_id", camera.name);
            teleportAvatarTo.Add("position", camera.position);
            commandBuilders.Add(teleportAvatarTo);
            // Look at the target position.
            CommandBuilder lookAtPosition = new CommandBuilder("look_at_position");
            lookAtPosition.Add("avatar_id", camera.name);
            lookAtPosition.Add("position", camera.lookAt);
            commandBuilders.Add(lookAtPosition);
        }
        // Get the root nodes.
        XElement rootElement = doc.Descendants("world").First();
        IEnumerable<XElement> modelElements = rootElement.Elements("model");
        int objectID = 0;
        Dictionary<string, int> objectIDs = new Dictionary<string, int>();
        Dictionary<string, LisdfAssetBundle> assetBundles = new Dictionary<string, LisdfAssetBundle>();
        foreach (XElement modelElement in modelElements)
        {
            Model model = new Model(modelElement.Attribute("name").Value.UriSafe(), modelElement, Path.GetDirectoryName(source), CoordinateSpace.ros, 1);
            models.Add(model.name, model);
            // Append commands.
            string assetBundlePath = Path.Combine(outputDirectory, model.name, GetPlatform(), model.name).FixWindowsPath();
            commandBuilders.Add(AddObjectCommand("add_object", model.name, assetBundlePath, model.pose, ref objectID, ref objectIDs));
            // Set the kinematic state of the model.
            if (model.isStatic)
            {
                CommandBuilder setKinematicState = new CommandBuilder("set_kinematic_state");
                setKinematicState.Add("id", objectIDs[model.name]);
                setKinematicState.Add("is_kinematic", true);
                setKinematicState.Add("gravity", true);
                commandBuilders.Add(setKinematicState);
            }
            bool assetBundlesExist;
            assetBundlesExist = true;
            foreach (string platform in Platforms)
            {
                if (!File.Exists(Path.Combine(outputDirectory, model.name, platform, model.name)))
                {
                    assetBundlesExist = false;
                    break;
                }
            }
            if (assetBundlesExist && !overwrite)
            {
                Log("Skipping " + model.name + " because there are already asset bundles.");
            }
            // Create the asset bundle.
            else
            {
                CompositeObjectCreator c = new CompositeObjectCreator(modelElement, source, Path.Combine(outputDirectory, model.name));
                if (!CreateAssetBundles<CompositeObjectCreator, ModelRecord>(c, cleanup))
                {
                    return;
                }
            }
        }
        // Add includes.
        IEnumerable<XElement> includeElements = rootElement.Elements("include");
        foreach (XElement includeElement in includeElements)
        {
            LisdfInclude include = new LisdfInclude(includeElement);
            string assetBundlePath = Path.Combine(outputDirectory, include.name, GetPlatform(), include.name).FixWindowsPath();
            // This is a robot.
            if (robots.ContainsKey(include.name))
            {
                Log(include.name + " is a robot.");
                RobotCreator creator = new RobotCreator(include.name, robots[include.name],
                    Path.Combine(outputDirectory, include.name));
                // Remember the model.
                Model model = creator.GetModel();
                models.Add(model.name, model);
                // Create an asset bundle.
                if (overwrite || !File.Exists(assetBundlePath))
                {
                    if (!CreateAssetBundles<RobotCreator, RobotRecord>(creator, cleanup))
                    {
                        return;
                    }
                }
                else
                {
                    Log("Asset bundle already exists: " + assetBundlePath);
                }
                LisdfAssetBundle assetBundle = new LisdfAssetBundle(assetBundlePath);
                assetBundles.Add(assetBundle.name, assetBundle);
                // Create an add_robot command.
                commandBuilders.Add(AddObjectCommand("add_robot", include.name, assetBundlePath, include.pose, ref objectID, ref objectIDs));
                if (include.isStatic)
                {
                    // Set the robot as immovable.
                    CommandBuilder setImmovable = new CommandBuilder("set_immovable");
                    setImmovable.Add("id", objectIDs[include.name]);
                    setImmovable.Add("immovable", true);
                    commandBuilders.Add(setImmovable);
                }
            }
            // This is a composite object.
            else
            {
                Log(include.name + " is a composite object.");
                CompositeObjectCreator creator = new CompositeObjectCreator(include.name, 
                    PathUtil.GetPathFrom(Path.GetDirectoryName(source), include.uri),
                    Path.Combine(outputDirectory, include.name), globalScale: include.scale);
                // Remember the model.
                Model model = creator.GetModel();
                models.Add(model.name, model);
                if (overwrite || !File.Exists(assetBundlePath))
                {
                    if (!CreateAssetBundles<CompositeObjectCreator, ModelRecord>(creator, cleanup))
                    {
                        return;
                    }
                }
                else
                {
                    Log("Asset bundle already exists: " + assetBundlePath);
                }
                LisdfAssetBundle assetBundle = new LisdfAssetBundle(assetBundlePath);
                assetBundles.Add(assetBundle.name, assetBundle);
                // Create an add_object command.
                commandBuilders.Add(AddObjectCommand("add_object", include.name, assetBundlePath, include.pose, ref objectID, ref objectIDs));
                // Set the kinematic state.
                if (include.isStatic)
                {
                    // Set Continuous Speculative collision detection mode for kinematic objects.
                    CommandBuilder setObjectCollisionDetectionMode = new CommandBuilder("set_object_collision_detection_mode");
                    setObjectCollisionDetectionMode.Add("id", objectIDs[include.name]);
                    setObjectCollisionDetectionMode.Add("mode", "continuous_speculative");
                    commandBuilders.Add(setObjectCollisionDetectionMode);
                    // Set the kinematic state. Some objects created by a CompositeObjectCreator won't actually be composite objects because they don't have joints.
                    CommandBuilder setKinematicState = new CommandBuilder(assetBundle.isCompositeObject ? "set_composite_object_kinematic_state" : "set_kinematic_state");
                    setKinematicState.Add("id", objectIDs[include.name]);
                    setKinematicState.Add("is_kinematic", true);
                    setKinematicState.Add("gravity", true);
                    commandBuilders.Add(setKinematicState);
                }
            }
        }
        // Add state commands.
        IEnumerable<XElement> stateElements = rootElement.Elements("state");
        foreach (XElement stateElement in stateElements)
        {
            IEnumerable<XElement> modelStateElements = stateElement.Elements("model");
            foreach (XElement modelStateElement in modelStateElements)
            {
                LisdfModelState modelState = new LisdfModelState(modelStateElement);
                if (!assetBundles.ContainsKey(modelState.name))
                {
                    Log("Warning! Couldn't find asset bundle joint data for: " + modelState.name);
                }
                else
                {
                    Log("Found model state: " + modelState.name);
                    LisdfAssetBundle assetBundle = assetBundles[modelState.name];
                    int parentID = objectIDs[modelState.name];
                    List<UrdfJoint> joints = models[modelState.name].joints;
                    foreach (string jointName in modelState.angles.Keys)
                    {
                        // Get the child link that corresponds to this joint.
                        string linkName = "";
                        bool gotLinkName = false;
                        for (int i = 0; i < joints.Count; i++)
                        {
                            if (joints[i].name == jointName)
                            {
                                linkName = joints[i].child;
                                gotLinkName = true;
                                break;
                            }
                        }
                        if (!gotLinkName)
                        {
                            Log("Warning! Failed to find link name corresponding to: " + jointName + " in model: " + modelState.name);
                            continue;
                        }
                        // Set the sub-object ID and the angle.
                        if (assetBundle.isModel)
                        {
                            // Get the rotation axis.
                            if (assetBundle.hingeJointAxes.ContainsKey(linkName))
                            {
                                Vector3 axis = assetBundle.hingeJointAxes[linkName];
                                string rotateAxis;
                                if (axis.x != 0)
                                {
                                    rotateAxis = "pitch";
                                }
                                else if (axis.y != 0)
                                {
                                    rotateAxis = "yaw";
                                }
                                else if (axis.z != 0)
                                {
                                    rotateAxis = "roll";
                                }
                                else
                                {
                                    Log("Error! Invalid joint axis: " + axis + " for joint: " + linkName + " for model " + modelState.name);
                                    return;
                                }
                                // Set the sub-object ID so that we can immediately rotate it.
                                CommandBuilder setSubObjectID = new CommandBuilder("set_sub_object_id");
                                setSubObjectID.Add("id", parentID);
                                setSubObjectID.Add("sub_object_id", objectID);
                                setSubObjectID.Add("sub_object_name", linkName);
                                commandBuilders.Add(setSubObjectID);
                                // Rotate the sub-object.
                                CommandBuilder rotateObjectBy = new CommandBuilder("rotate_object_by");
                                rotateObjectBy.Add("id", objectID);
                                rotateObjectBy.Add("angle", modelState.angles[jointName]);
                                rotateObjectBy.Add("axis", rotateAxis);
                                rotateObjectBy.Add("is_world", false);
                                commandBuilders.Add(rotateObjectBy);
                            }
                            else
                            {
                                Log("Error! Invalid joint: " + jointName + " for model " + modelState.name);
                                return;
                            }
                        }
                        // Set the joint ID and the angle.
                        else
                        {
                            // Set the joint ID.
                            CommandBuilder setRobotJointID = new CommandBuilder("set_robot_joint_id");
                            setRobotJointID.Add("id", parentID);
                            setRobotJointID.Add("joint_id", objectID);
                            setRobotJointID.Add("joint_name", linkName);
                            commandBuilders.Add(setRobotJointID);
                            CommandBuilder setRevoluteAngle = new CommandBuilder("set_revolute_angle");
                            setRevoluteAngle.Add("id", parentID);
                            setRevoluteAngle.Add("joint_id", objectID);
                            setRevoluteAngle.Add("angle", modelState.angles[jointName]);
                            commandBuilders.Add(setRevoluteAngle);
                        }
                        // Increment the next joint ID.
                        objectID++;
                    }
                }
            }
        }
        // Build the string of commands.
        string commands = "[";
        for (int i = 0; i < commandBuilders.Count; i++)
        {
            commands += commandBuilders[i].End();
            // Add a comma between commands.
            if (i < commandBuilders.Count - 1)
            {
                commands += ",\n";
            }
        }
        commands += "]";
        Log(commands);
        string commandsPath = Path.Combine(outputDirectory, "commands.json").FixWindowsPath();
        Log("Saved commands to: " + commandsPath);
        File.WriteAllText(commandsPath, commands);
    }


    /// <summary>
    /// Append commands to add an object.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="path">The path to the asset bundle.</param>
    /// <param name="pose">The pose.</param>
    /// <param name="objectID">The object ID.</param>
    /// <param name="objectIDs">The overall dictionary of object IDs.</param>
    private static CommandBuilder AddObjectCommand(string commandName, string name, string path, Pose pose, ref int objectID,
        ref Dictionary<string, int> objectIDs)
    {
        CommandBuilder cb = new CommandBuilder(commandName);
        cb.Add("name", name);
        cb.Add("id", objectID);
        cb.Add("url", "file:///" + path.FixWindowsPath());
        cb.Add("position", pose.position);
        cb.Add("rotation", pose.eulerAngles);
        // Remember the object ID.
        objectIDs.Add(name, objectID);
        // Increment the ID for the next object.
        objectID++;
        return cb;
    }


    /// <summary>
    /// Returns the string name of the platform.
    /// </summary>
    private static string GetPlatform()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            return "Windows";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            return "Darwin";
        }
        else if (Application.platform == RuntimePlatform.LinuxEditor)
        {
            return "Linux";
        }
        else
        {
            throw new Exception(Application.platform.ToString());
        }
    }


    /// <summary>
    /// Returns the infix of a platform-specific asset bundle.
    /// </summary>
    private static string GetAssetBundleInfix()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            return "windows";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            return "osx";
        }
        else if (Application.platform == RuntimePlatform.LinuxEditor)
        {
            return "linux";
        }
        else
        {
            throw new Exception(Application.platform.ToString());
        }
    }


    /// <summary>
    /// Create an asset bundle.
    /// </summary>
    /// <param name="creator">The creator.</param>
    /// <param name="cleanup">If true, cleanup when we're done.</param>
    private static bool CreateAssetBundles<T, U>(T creator, bool cleanup)
        where T : AssetBundleCreator<T, U>, new()
        where U: Record
    {
        // Set the log path to the combined log.
        creator.logPath = logPath;
        if (!creator.CreatePrefab())
        {
            Logger.StopLogging();
            // Log the error.
            Log("Failed to create: " + creator.name);
            return false;
        }
        creator.CreateAssetBundles();
        Logger.StopLogging();
        if (cleanup)
        {
            PathUtil.Cleanup();
        }
        return true;
    }


    /// <summary>
    /// Force the debug message to write to the log file.
    /// </summary>
    /// <param name="message">The message.</param>
    private static void Log(string message)
    {
        Logger.StartLogging(logPath);
        Debug.Log(message);
        Logger.StopLogging();
    }
}