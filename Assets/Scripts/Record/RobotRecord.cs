using System.Collections.Generic;


/// <summary>
/// A robot metadata record.
/// </summary>
public class RobotRecord : Record
{
    /// <summary>
    /// Cached IK data. Leave this empty.
    /// </summary>
    public object[] ik;
    /// <summary>
    /// If true, the root object of the robot is immovable.
    /// </summary>
    public bool immovable;
    /// <summary>
    /// The source of the robot.
    /// </summary>
    public string source;
    /// <summary>
    /// Initial joint targets. Leave this empty.
    /// </summary>
    public Dictionary<object, object> targets;
    /// <summary>
    /// The up axis.
    /// </summary>
    public string up = "y";
}