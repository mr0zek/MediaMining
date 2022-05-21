using System;
using System.Collections.Generic;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class LocationDescription
  {
    public IList<MediaDescription> Media { get; set; } = new List<MediaDescription>();
    public CoordinatesDescription Coordinates { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public LocationDescription(CoordinatesDescription coordinates, string name, string country, DateTime dateFrom, DateTime dateTo)
    {
      Coordinates = coordinates;
      Name = name;
      Country = country;
      DateFrom = dateFrom;
      DateTo = dateTo;
    }

    public bool CanBeAssigned(CoordinatesDescription coordinatesDescription)
    {
      if (Coordinates.DistanceTo(Coordinates) < 10)
      {
        return true;
      }

      return false;
    }
  }
}