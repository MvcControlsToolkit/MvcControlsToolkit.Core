using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using MvcControlsToolkit.Core.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class WeekJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) serializer.Serialize(writer, null);
            else serializer.Serialize(writer, ((Week)value).StartDate() as object, typeof(DateTime));

        }
        public override bool CanConvert(Type objectType)
        {
            return (Nullable.GetUnderlyingType(objectType) ?? objectType) == typeof(Week);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DateTime? result = serializer.Deserialize(reader, typeof(DateTime?)) as DateTime?;
            if (result == null) return null;
            else return Week.FromDateTime(result.Value);
        }
    }
}
