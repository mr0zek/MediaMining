using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPrep.GoogleJson
{
  public class GoogleJsonRecords 
  {
    public List<Location> Locations;

    public void FilterLocations(DateTime from, DateTime to)
    {
      Locations = Locations.Where(f => f.Date < to && f.Date > from).ToList();
    }
  }
}