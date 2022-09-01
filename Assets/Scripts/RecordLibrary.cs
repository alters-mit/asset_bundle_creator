using System.Collections.Generic;


/// <summary>
/// A library of records.
/// </summary>
public class RecordLibrary<T>
    where T: Record
{
    /// <summary>
    /// The records.
    /// </summary>
    public Dictionary<string, T> records;
    /// <summary>
    /// The library description.
    /// </summary>
    public string description;
}