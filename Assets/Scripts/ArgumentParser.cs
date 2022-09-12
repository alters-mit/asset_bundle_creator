using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using SubalternGames;


/// <summary>
/// Parser for command-line arguments.
/// </summary>
public static class ArgumentParser
{
    /// <summary>
    /// Regular expression used to parse arguments with values.
    /// </summary>
    private const string REGEX_ARGUMENTS = "-(.*?)=(.*)";


    /// <summary>
    /// A dictionary of args. Key = Flag. Value = Argument value.
    /// </summary>
    private static Dictionary<string, string> args;
    /// <summary>
    /// A set of arguments with no values (i.e. the value is true if the flag exists and false if it doesn't).
    /// </summary>
    private static HashSet<string> booleanArgs;


    static ArgumentParser()
    {
        args = new Dictionary<string, string>();
        booleanArgs = new HashSet<string>();
        // Get the arguments.
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        foreach (string arg in commandLineArgs)
        {
            // Check if the argument is formatted like -flag="value".
            MatchCollection matches = Regex.Matches(arg, REGEX_ARGUMENTS);
            // If the argument isn't formatted that way, assume it's a boolean flag like -flag.
            int numMatches = matches.Count;
            if (numMatches == 0)
            {
                booleanArgs.Add(arg);
            }
            else
            {
                for (int i = 0; i < numMatches; i++)
                {
                    Match match = matches[i];
                    // Get the flag and the value.
                    string flag = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    // Remove any double quotes enclosing the argument.
                    if (Application.platform == RuntimePlatform.LinuxEditor)
                    {
                        value = value.Replace("\"", "");
                    }
                    else
                    {
                        value = value.Replace("\\\"", "");
                    }
                    args.Add(flag, value);
                }
            }
        }
    }


    /// <summary>
    /// Try to get the value of an argument.
    /// </summary>
    /// <param name="flag">The argument flag.</param>
    /// <param name="defaultValue">The default value if the flag is not present.</param>
    public static string TryGet(string flag, string defaultValue)
    {
        if (!args.ContainsKey(flag))
        {
            return defaultValue;
        }
        else
        {
            return args[flag];
        }
    }


    /// <summary>
    /// Returns the string value of the argument.
    /// </summary>
    /// <param name="flag">The argument flag.</param>
    public static string Get(string flag)
    {
        try
        {
            return args[flag];
        }
        catch (KeyNotFoundException e)
        {
            UnityEngine.Debug.LogError(flag);
            throw e;
        }
    }


    /// <summary>
    /// Try to get the value of an argument.
    /// </summary>
    /// <param name="flag">The argument flag.</param>
    /// <param name="defaultValue">The default value if the flag is not present.</param>
    public static int TryGet(string flag, int defaultValue)
    {
        if (!args.ContainsKey(flag))
        {
            return defaultValue;
        }
        else
        {
            int v;
            if (int.TryParse(args[flag], out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }
    }


    /// <summary>
    /// Try to get the value of an argument.
    /// </summary>
    /// <param name="flag">The argument flag.</param>
    /// <param name="defaultValue">The default value if the flag is not present.</param>
    public static float TryGet(string flag, float defaultValue)
    {
        if (!args.ContainsKey(flag))
        {
            return defaultValue;
        }
        else
        {
            float v;
            if (float.TryParse(args[flag], out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }
    }


    /// <summary>
    /// Deserialize a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    /// <param name="flag">The argument flag.</param>
    /// <param name="defaultValue">The default value if the flag is not present.</param>
    public static T TryGet<T>(string flag, T defaultValue)
    {
        if (!args.ContainsKey(flag))
        {
            return defaultValue;
        }
        else
        {
            // Replace ' with " and deserialize.
            return JsonWrapper.Deserialize<T>(args[flag].Replace('\'', '"'));
        }
    }


    /// <summary>
    /// Returns true if the boolean argument is true.
    /// </summary>
    /// <param name="arg">The command-line flag.</param>
    public static bool GetBoolean(string arg)
    {
        return booleanArgs.Contains(arg);
    }
}
