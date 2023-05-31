using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaMining.EventsImporter
{
  public class EventData
  {
    public DateTime DateFrom{ get; set; }
    public DateTime DateTo { get; set; }
    public string Name { get; set; }
          
    internal static EventData FromFile(FilePath filePath)
    {
      return JsonConvert.DeserializeObject<EventData>(File.ReadAllText(filePath));
    }
  }
}