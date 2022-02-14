using System;
using System.Collections.Generic;

namespace MediaPreprocessor.Excursions
{
  public class ExcursionsRoot
  {
    public List<Excursion> Excursions { get; set; } = new List<Excursion>();

    public Excursion GetEvent(DateTime date)
    {
      foreach (var @event in Excursions)
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