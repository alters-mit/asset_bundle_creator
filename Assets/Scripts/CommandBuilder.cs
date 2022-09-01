using System.Text;
using UnityEngine;


/// <summary>
/// Build a command using a StringBuilder.
/// </summary>
public class CommandBuilder
{
    /// <summary>
    /// The StringBuilder used for the command.
    /// </summary>
    private StringBuilder sb = new StringBuilder();


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="type">The type of command e.g. "add_object".</param>
    public CommandBuilder(string type)
    {
        // Start the command.
        sb.Append("{");
        // Add the type.
        Add("$type", type);
    }


    /// <summary>
    /// Add a string parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="value">The string value.</param>
    public void Add(string parameter, string value)
    {
        AddParameter(parameter);
        sb.Append("\"" + value + "\", ");
    }


    /// <summary>
    /// Add an integer parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="value">The integer value.</param>
    public void Add(string parameter, int value)
    {
        AddParameter(parameter);
        sb.Append(value + ", ");
    }


    /// <summary>
    /// Add an float parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="value">The float value.</param>
    public void Add(string parameter, float value)
    {
        AddParameter(parameter);
        sb.Append(value + ", ");
    }


    /// <summary>
    /// Add an boolean parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="value">The boolean value.</param>
    public void Add(string parameter, bool value)
    {
        AddParameter(parameter);
        sb.Append((value ? "true" : "false") + ", ");
    }


    /// <summary>
    /// Add an Vector3 parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="value">The Vector3 value.</param>
    public void Add(string parameter, Vector3 value)
    {
        AddParameter(parameter);
        sb.Append(value.ToJsonString() + ", ");
    }


    /// <summary>
    /// End the command. Returns the string builder.
    /// </summary>
    public string End()
    {
        string command = sb.ToString().Trim();
        if (command.EndsWith(","))
        {
            command = command.Substring(0, command.Length - 1);
        }
        command += "}";
        return command;
    }


    /// <summary>
    /// Add a parameter.
    /// </summary>
    /// <param name="parameter">The parameter name.</param>
    private void AddParameter(string parameter)
    {
        sb.Append("\"" + parameter + "\": ");
    }
}