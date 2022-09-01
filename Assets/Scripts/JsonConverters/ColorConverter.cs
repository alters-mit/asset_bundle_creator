using UnityEngine;
using Newtonsoft.Json;
using System;


/// <summary>
/// Source: https://gist.githubusercontent.com/zcyemi/e0c71c4f8ba8a92944a17f888253db0d/raw/a7523e46ba4f7d703ee657076942c90b42e5968a/JsonHelper.cs
/// </summary>
public class ColorConverter : JsonConverter<Color>
{
    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var t = serializer.Deserialize(reader);
        var iv = JsonConvert.DeserializeObject<Color>(t.ToString());
        return iv;
    }


    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("r");
        writer.WriteValue(value.r);
        writer.WritePropertyName("g");
        writer.WriteValue(value.g);
        writer.WritePropertyName("b");
        writer.WriteValue(value.b);
        writer.WritePropertyName("a");
        writer.WriteValue(value.a);
        writer.WriteEndObject();
    }
}