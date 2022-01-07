using System;

namespace GoogleTakeoutProcessor
{
  class EventsRoot
  {
    public Event[] Events { get; set; }

    public string GetEventName(DateTime date)
    {
      foreach (var @event in Events)
      {
        if (@event.InEvent(date))
        {
          return $"{@event.DateFrom.ToString("yyyy-MM-dd")} - {@event.Name}";
        }
      }

      return null;
    }
  }
}