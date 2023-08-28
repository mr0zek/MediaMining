using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Events
{
  public class Event
  {
    [JsonProperty("DateFrom")]
    private string _dateFrom;
    [JsonProperty("DateTo")]
    private string _dateTo;
    public string Name { get; set; }

    [JsonIgnore]
    public Date DateFrom
    {
      get => _dateFrom;
      set => _dateFrom = value;
    }

    [JsonIgnore]
    public Date DateTo
    {
      get => _dateTo;
      set => _dateTo = value;
    }

    public List<Day> Days { get; set; } = new List<Day>();

    public List<Note> Notes { get; set; } = new List<Note>();

    [JsonIgnore]
    public EventId Id
    {
      get
      {
        return new EventId(GetUniqueName());
      }
    }

    public bool InEvent(Date date)
    {
      return date <= DateTo && date >= DateFrom;
    }

    public string GetUniqueName()
    {
      return $"{DateFrom} - {Name}";
    }

    public Day GetDay(Date date)
    {
      if(date < DateFrom || date > DateTo)
      {
        throw new ArgumentException(date);
      }

      var d =  Days.FirstOrDefault(f => f.Date == date);
      if (d == null)
      {
        Days.Add(d = new Day(){Date = date});
      }

      return d;
    }

    internal void Merge(Event ev)
    {
      if (ev.Name != null)
      {
        Name = ev.Name;
      }
      if (ev.DateFrom != DateTime.MinValue)
      {
        DateFrom = ev.DateFrom;
      }
      if (ev.DateTo != DateTime.MinValue)
      {
        DateTo = ev.DateTo;
      }
      if (ev.Days.Count() > 0)
      {
        Days = ev.Days;
      }
    }
  }
}