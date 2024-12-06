using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GeoJSON.Net.Feature;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.MapGenerator;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MediaPreprocessor.Events
{
  public class EventRepository : IEventRepository
  {
    private readonly DirectoryPath _eventsPath;
    private readonly IDictionary<EventId, Event> _events = new Dictionary<EventId, Event>();

    public EventRepository(
      string eventsPath)
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
      foreach (Event e in _events.Values)
      {
        File.WriteAllText(GetFilePath(e), JsonConvert.SerializeObject(e, Formatting.Indented));
      }
    }

    private DirectoryPath GetFilePath(Event e)
    {
      return _eventsPath
        .AddDirectory(e.DateFrom.Year.ToString())
        .AddDirectory(e.DateFrom.ToString("yyyy-MM-dd") + " - " + e.Name);
    }

    public void LoadFromPath(DirectoryPath eventsPath)
    {
      if (eventsPath.Exists)
      {
        var files = Directory.GetFiles(eventsPath, "*.json", SearchOption.AllDirectories);
        foreach (string file in files)
        {
          try
          {
            Event @event = Event.FromFile(file);
            _events.Add(@event.Id, @event);
          }
          catch(Exception ex)
          {
            Console.WriteLine($"Error in file: {file}"+ex.ToString());
            throw;
          }
        }
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
      var directoryPath = GetFilePath(ev);
      directoryPath.Create();

      File.WriteAllText(directoryPath.ToFilePath(ev.DateFrom.ToString("yyyy-MM-dd") + " - " + ev.Name + ".event"), JsonConvert.SerializeObject(ev, Formatting.Indented));      
    }
  }
}