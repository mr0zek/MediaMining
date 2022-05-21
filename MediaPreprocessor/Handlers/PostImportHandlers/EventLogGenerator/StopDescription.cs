using System;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class StopDescription
  {
    public CoordinatesDescription Coordinates { get; }
    public string LocationName { get; set; }
    public string Country { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public StopDescription(CoordinatesDescription coordinates, string locationName, string country,
      Date dateFrom, Date dateTo)
    {
      Coordinates = coordinates;
      LocationName = locationName;
      Country = country;
      DateFrom = dateFrom;
      DateTo = dateTo;
    }
  }
}