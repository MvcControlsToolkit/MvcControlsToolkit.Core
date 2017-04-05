using System;
using Newtonsoft.Json;


namespace MvcControlsToolkit.Core.Types
{
    public class MonthJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) serializer.Serialize(writer, null);
            else serializer.Serialize(writer, ((Month)value).ToDateTime() as object, typeof(DateTime)); 
            
        }
        public override bool CanConvert(Type objectType)
        {
            return (Nullable.GetUnderlyingType(objectType)??objectType) == typeof(Month);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DateTime? result = serializer.Deserialize(reader, typeof(DateTime?)) as DateTime? ;
            if (result == null) return null;
            else return Month.FromDateTime(result.Value);
        }
    }
}
