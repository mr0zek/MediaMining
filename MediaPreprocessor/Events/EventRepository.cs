using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MediaPreprocessor.Events
{
  class EventRepository : IEventRepository
  {
    private readonly DirectoryPath _eventsPath;
    private readonly IDictionary<EventId, Event> _events = new Dictionary<EventId, Event>();

    public EventRepository(string eventsPath)
    {
      _eventsPath = eventsPath;
      LoadFromPath(eventsPath);
    }

    public Event GetByDate(Date date)
    {
      return _events.Values.FirstOrDefault(f=>f.InEvent(date));
    }

    public Event Get(EventId eventId)
    {
      return _events[eventId];
    }

    public void Save()
    {
      _eventsPath.DeleteAllInside();

      foreach (Event e in _events.Values)
      {
        File.WriteAllText(GetFilePath(e), JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }

    private string GetFilePath(Event e)
    {
      return _eventsPath.ToFilePath(e.DateFrom.ToString("yyyy-MM-dd") + " - " + e.Name + ".event");
    }

    public void LoadFromPath(DirectoryPath eventsPath)
    {
      EventsRoot result = new EventsRoot();
      var files = Directory.GetFiles(eventsPath, "*.event", SearchOption.AllDirectories);
      foreach (string file in files)
      {
        Event @event = JsonConvert.DeserializeObject<Event>(File.ReadAllText(file), new JsonDateConverter());
        _events.Add(@event.Id, @event);        
      }
    }

    public void Add(Event ev)
    {
      if (_events.ContainsKey(ev.Id))
      {
        _events[ev.Id].Merge(ev);
      }
      else
      {
        _events.Add(ev.Id, ev);
      }

      File.WriteAllText(GetFilePath(ev), JsonConvert.SerializeObject(ev, Formatting.Indented));
    }
  }
}