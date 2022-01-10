using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Events
{
  public class EventsRoot
  {
    public List<Event> Events { get; set; } = new List<Event>();

    public string GetEventName(DateTime date)
    {
      foreach (var @event in Events)
      {
        if (@event.InEvent(date))
        {
          return $"{@event.DateFrom:yyyy-MM-dd} - {@event.Name}";
        }
      }

      return null;
    }

    public static EventsRoot LoadFromPath(string eventsPath)
    {
      if (File.Exists(eventsPath))
      {
        return JsonConvert.DeserializeObject<EventsRoot>(File.ReadAllText(eventsPath));
      }
      EventsRoot result = new EventsRoot();
      var files = Directory.GetFiles(eventsPath, "*.json", SearchOption.AllDirectories);
      foreach (string file in files)
      {
        EventsRoot r = JsonConvert.DeserializeObject<EventsRoot>(File.ReadAllText(file));
        result.MergeFrom(r);
      }

      return result;
    }

    private void MergeFrom(EventsRoot eventsRoot)
    {
      Events.AddRange(eventsRoot.Events);
    }
  }
}