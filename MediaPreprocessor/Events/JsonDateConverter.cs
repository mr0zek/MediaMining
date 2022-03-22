using System;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Events
{
  public class JsonDateConverter : JsonConverter
  {
    public override object ReadJson(JsonReader reader,
      Type objectType,
      object existingValue,
      JsonSerializer serializer)
    {
      return Date.Parse(reader.Value.ToString());
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(Date).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      writer.WriteValue(value.ToString());
    }
  }
}