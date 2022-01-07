using System;

namespace GoogleTakeoutProcessor
{
  class Event
  {
    public string Name { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public bool InEvent(DateTime date)
    {
      return date.Date <= DateTo && date.Date >= DateFrom;
    }
  }
}