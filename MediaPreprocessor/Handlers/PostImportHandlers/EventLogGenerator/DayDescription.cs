using System;
using System.Collections;
using System.Collections.Generic;
using MediaPreprocessor.Positions.StopDetection;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class DayDescription
  {
    public DateTime Date { get; set; }

    public IList<MediaDescription> Media { get; set; } = new List<MediaDescription>();

    public IList<TrackDescription> Tracks { get; set; } = new List<TrackDescription>();
    public IList<StopDescription> Stops { get; set; } = new List<StopDescription>();

    public DayDescription(Date date)
    {
      Date = date;
    }

    public IEnumerable<string> GetVisitedCountries()
    {
      HashSet<string> result = new ();
      foreach (var stop in Stops)
      {
        if (!result.Contains(stop.Country))
        {
          result.Add(stop.Country);
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
      foreach (var stop in Stops)
      {
        if (!result.Contains(stop.LocationName))
        {
          result.Add(stop.LocationName);
        }
      }

      return result;
    }

    public void AddMedia(MediaDescription mediaDescription)
    {
      Media.Add(mediaDescription);
    }

    public void AddStop(StopDescription stop)
    {
      Stops.Add(stop);
    }
  }
}