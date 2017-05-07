using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class RuntimeTypesJsonConverter: JsonConverter
    {
        protected static JsonReader CopyReaderForObject(JsonReader reader, JObject jObject)
        {
            JsonReader jObjectReader = jObject.CreateReader();
            jObjectReader.Culture = reader.Culture;
            jObjectReader.DateFormatString = reader.DateFormatString;
            jObjectReader.DateParseHandling = reader.DateParseHandling;
            jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
            jObjectReader.FloatParseHandling = reader.FloatParseHandling;
            jObjectReader.MaxDepth = reader.MaxDepth;
            jObjectReader.SupportMultipleContent = reader.SupportMultipleContent;
            return jObjectReader;
        }
        public override bool CanConvert(Type objectType)
        {
            return true;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t=JToken.FromObject(value);
            if (value != null && t.Type == JTokenType.Object)
            {
                var code = DerivedClassesRegister.GetCodeFromType(value.GetType());
                if(code != null)
                {
                    (t as JObject).AddFirst(new JProperty("$type", code));
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
            jo.TryGetValue("$type", out value);
            var type= DerivedClassesRegister.GetTypeFromCode(value.Value<string>());
            if (type != null && !objectType.GetTypeInfo().IsAssignableFrom(type)) type = null;
            var target = Activator.CreateInstance(type ?? objectType);
            // Populate the object properties
            using (JsonReader jObjectReader = CopyReaderForObject(reader, jo))
            {
                serializer.Populate(jObjectReader, target);
            }
            return target;
        }
    }
}
