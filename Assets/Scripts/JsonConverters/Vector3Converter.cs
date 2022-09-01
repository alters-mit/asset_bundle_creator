using UnityEngine;
using Newtonsoft.Json;
using System;


/// <summary>
/// Source: https://gist.githubusercontent.com/zcyemi/e0c71c4f8ba8a92944a17f888253db0d/raw/a7523e46ba4f7d703ee657076942c90b42e5968a/JsonHelper.cs
/// </summary>
public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var t = serializer.Deserialize(reader);
        var iv = JsonConvert.DeserializeObject<Vector3>(t.ToString());
        return iv;
    }


    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WriteEndObject();
    }
}