using System;
using System.Collections.Generic;

namespace MediaPreprocessor.Events
{
  public class EventsRoot
  {
    public List<Event> Events { get; set; } = new List<Event>();

    public Event GetEvent(DateTime date)
    {
      foreach (var @event in Events)
      {
        if (@event.InEvent(date))
        {
          return @event;
        }
      }

      return null;
    }
  }
}