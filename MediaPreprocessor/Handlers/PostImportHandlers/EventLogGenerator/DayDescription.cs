using System;
using System.Collections;
using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class DayDescription
  {
    public DateTime Date { get; set; }

    public IList<TrackDescription> Tracks { get; set; } = new List<TrackDescription>();
    public IList<StopDescription> Stops { get; set; } = new List<StopDescription>();
    public IList<LocationDescription> Locations { get; set; } = new List<LocationDescription>();

    public DayDescription(Date date)
    {
      Date = date;
    }

    public IEnumerable<string> GetVisitedCountries()
    {
      HashSet<string> result = new ();
      foreach (var location in Locations)
      {
        if (!result.Contains(location.Country))
        {
          result.Add(location.Country);
        }
      }

      return result;
    }

    public double GetDistance()
    {
      double distance = 0;
      foreach (var track in Tracks)
      {
        distance += track.Distance;
      }

      return Math.Round(distance,0);
    }

    public IEnumerable<string> GetVisitedPlaces()
    {
      HashSet<string> result = new();
      foreach (var location in Locations)
      {
        if (!result.Contains(location.Name))
        {
          result.Add(location.Name);
        }
      }

      return result;
    }

    public void AddMedia(MediaDescription mediaDescription)
    {
      foreach (var location in Locations)
      {
        if (location.CanBeAssigned(mediaDescription.Coordinates))
        {
          location.Media.Add(mediaDescription);
          return;
        }
      }

      //throw new InvalidOperationException($"Cannot assign media: {mediaDescription.Path} to location");
    }

    public void AddStop(StopDescription stop)
    {
      Locations.Add(new LocationDescription(stop.Coordinates, stop.LocationName, stop.Country, stop.DateFrom,
        stop.DateTo));
      Stops.Add(stop);
    }
  }
}