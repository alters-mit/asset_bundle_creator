using Newtonsoft.Json;
using System.IO;
using UnityEngine;


namespace SubalternGames
{
    /// <summary>
    /// Convenient wrapper for Newtonsoft.Json.
    /// Usage:
    /// 
    /// MyClass o = JsonWrapper.DeserializeFromPath<MyClass>("C:/save.json");
    /// JsonWrapper.Serialize(o, "C:/save.json");
    /// 
    ///
    /// MIT License
    ///
    /// Copyright (c) 2021 Seth Alter
    ///
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    ///
    /// The above copyright notice and this permission notice shall be included in all
    /// copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    /// SOFTWARE.
    /// </summary>
    public static class JsonWrapper
    {
        /// <summary>
        /// JSON serializer settings. Never call this directly!
        /// </summary>
        private static JsonSerializerSettings serializerSettings;
        /// <summary>
        /// JSON serializer settings with type handling. Never call this directly!
        /// </summary>
        private static JsonSerializerSettings serializerSettingsWithTypeHandling;
        /// <summary>
        /// JSON serializer. Never call this directly!
        /// </summary>
        private static JsonSerializer serializer;
        /// <summary>
        /// JSON serializer with type handling. Never call this directly!
        /// </summary>
        private static JsonSerializer serializerWithTypeHandling;
        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        private static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (serializerSettings == null)
                {
                    serializerSettings = GetJsonSerializerSettings(TypeNameHandling.None);
                }
                return serializerSettings;
            }
        }
        /// <summary>
        /// JSON serializer settings with type handling.
        /// </summary>
        private static JsonSerializerSettings SerializerSettingsWithTypeHandling
        {
            get
            {
                if (serializerSettingsWithTypeHandling == null)
                {
                    serializerSettingsWithTypeHandling = GetJsonSerializerSettings(TypeNameHandling.Auto);
                }
                return serializerSettingsWithTypeHandling;
            }
        }
        /// <summary>
        /// JSON serializer.
        /// </summary>
        private static JsonSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {

                    serializer = JsonSerializer.Create(SerializerSettings);
                }
                return serializer;
            }
        }
        /// <summary>
        /// JSON serializer.
        /// </summary>
        private static JsonSerializer SerializerWithTypeHandling
        {
            get
            {
                if (serializerWithTypeHandling == null)
                {

                    serializerWithTypeHandling = JsonSerializer.Create(SerializerSettingsWithTypeHandling);
                }
                return serializerWithTypeHandling;
            }
        }


        /// <summary>
        /// Serialize an object of type T to a file.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="o">The object.</param>
        /// <param name="path">The absolute path to the file (including the file extension).</param>
        /// <param name="typeHandling">Include type handling.</param>
        public static void Serialize<T>(T o, string path, bool typeHandling)
        {
            // Create the directory if needed.
            FileInfo fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            // Serialize the object and write it to disk.
            using (StreamWriter sw = new StreamWriter(path))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    if (typeHandling)
                    {
                        Serializer.Serialize(writer, o);
                    }
                    else
                    {
                        SerializerWithTypeHandling.Serialize(writer, o);
                    }
                }
            }
        }


        /// <summary>
        /// Serialize an object to a string.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="o">The object.</param>
        /// <param name="typeHandling">Include type handling.</param>
        public static string GetSerialized<T>(T o, bool typeHandling)
        {
            if (typeHandling)
            {
                return JsonConvert.SerializeObject(o, SerializerSettings).Replace("\r", "");
            }
            else
            {
                return JsonConvert.SerializeObject(o, SerializerSettingsWithTypeHandling).Replace("\r", "");
            }
        }


        /// <summary>
        /// Deserialize a serialized object of type T.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="o">The serialized object.</param>
        public static T Deserialize<T>(string o)
        {
            using (StringReader sr = new StringReader(o))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    return Serializer.Deserialize<T>(reader);
                }
            }
        }


        /// <summary>
        /// Deserialize an object of type T stored as a text file.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="path">The absolute path to the text file (including the file extension).</param>
        public static T DeserializeFromPath<T>(string path)
        {
            return Deserialize<T>(File.ReadAllText(path));
        }


        /// <summary>
        /// Deserialize an object of type T stored as a text file in Resources.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="path">The path to the file in Resources (minus the file extension).</param>
        public static T DeserializeFromResources<T>(string path)
        {
            return Deserialize<T>(Resources.Load<TextAsset>(path).text);
        }


        /// <summary>
        /// Returns JSON serializer settings.
        /// </summary>
        /// <param name="typeNameHandling">The type handling.</param>
        private static JsonSerializerSettings GetJsonSerializerSettings(TypeNameHandling typeNameHandling)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            // Ignore loops of type references.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.TypeNameHandling = typeNameHandling;
            // Make the output look nice.
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new ColorConverter());
            settings.Converters.Add(new QuaternionConverter());
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return settings;
        }
    }
}