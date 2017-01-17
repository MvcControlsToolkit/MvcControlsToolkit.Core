using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using MvcControlsToolkit.Core.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class ObjectJsonConverter: JsonConverter
    {
        private static HashSet<string> allowedTypeNames= new HashSet<string>(new string[] {
            typeof(int).Name,
            typeof(uint).Name,
            typeof(short).Name,
            typeof(ushort).Name,
            typeof(long).Name,
            typeof(ulong).Name,
            typeof(byte).Name,
            typeof(sbyte).Name,
            typeof(float).Name,
            typeof(double).Name,
            typeof(decimal).Name,
            typeof(DateTime).Name,
            typeof(DateTimeOffset).Name,
            typeof(TimeSpan).Name,
            typeof(Month).Name,
            typeof(Week).Name,
            typeof(string).Name,
            typeof(char).Name,
            typeof(Guid).Name
        });
        

        public override bool CanConvert(Type objectType)
        {
           return objectType == typeof(object);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var type = value.GetType();
            var t = new JObject();
            t.Add("$type", type.Name);
            if (allowedTypeNames.Contains(type.Name))
            {
                using (var s = new StringWriter())
                {
                    serializer.Serialize(s, value, type);
                    t.Add("$value", s.ToString());
                }
            }
            t.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            // Load JObject from stream
            JObject jo = JObject.Load(reader);
            // Create target object based on JObject
            JToken value = null;
            if (!jo.TryGetValue("$type", out value)) return null;
            var typeName = value.Value<string>();
            if (!allowedTypeNames.Contains(typeName)) return null;
            var type = Type.GetType("System."+ typeName);
            if (type == null) return null;
            if(!jo.TryGetValue("$value", out value)) return null;
            // Populate the object properties
            using (var s = new StringReader(value.Value<string>()))
            {
                return serializer.Deserialize(s, type);
            }
            
        }
    }
}
