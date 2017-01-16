using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class ObjectJsonConverter: JsonConverter
    {
        
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
            ;
            using (var s = new StringWriter())
            {
                serializer.Serialize(s, value, type);
                t.Add("$value", s.ToString());
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
            var type = Type.GetType("System."+value.Value<string>());
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
