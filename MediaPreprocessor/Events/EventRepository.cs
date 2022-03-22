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
    private readonly IDictionary<EventId, Event> _events = new Dictionary<EventId, Event>();

    public EventRepository(string eventsPath)
    {
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

    public void LoadFromPath(string eventsPath)
    {
      EventsRoot result = new EventsRoot();
      var files = Directory.GetFiles(eventsPath, "*.json", SearchOption.AllDirectories);
      foreach (string file in files)
      {
        EventsRoot r = JsonConvert.DeserializeObject<EventsRoot>(File.ReadAllText(file), new JsonDateConverter());
        foreach (var @event in r.Events)
        {
          _events.Add(@event.Id, @event);
        }
      }
    }
  }
}