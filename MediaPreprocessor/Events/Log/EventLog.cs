using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Events.Log
{
  public class EventLog
  {
    Dictionary<Date, IEnumerable<Stop>> _stops = new();
    readonly Dictionary<Date, Track> _tracksByDay = new();
    private readonly IGeolocation _geolocation;
    private readonly IStopDetector _stopDetector;
    private readonly ISet<string> _visitedCountries = new HashSet<string>();
    private double _distance;

    internal EventLog(Event @event, IGeolocation geolocation, IStopDetector stopDetector)
    {
      Event = @event;
      _geolocation = geolocation;
      _stopDetector = stopDetector;
    }

    public Event Event { get; }

    public void AddDayTrack(Date date, Track track)
    {
      _tracksByDay.Add(date, track);
    }

    public void PostProcess()
    {
      // Detect Stops
      _stops = _stopDetector.Detect(_tracksByDay.Values.SelectMany(f=>f.Positions))
        .GroupBy(f=>new Date(f.DateFrom))
        .ToDictionary(f=>new Date(f.Key), f=>f as IEnumerable<Stop>);
        
      
      // Smooth tracks

      // Calculate Stats
      _distance = _tracksByDay.Sum(f => f.Value.CalculateDistance());
    }    

    public void WriteDescription(FilePath fileName, IDictionary<Date, IEnumerable<Media.Media>> media)
    {
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.AppendLine($"# {Event.Name} ({Event.DateFrom} - {Event.DateTo})");

      stringBuilder.AppendLine("## Stats");
      stringBuilder.AppendLine($"- duration - {(Event.DateTo - Event.DateFrom).Days} days");
      stringBuilder.AppendLine($"- distance - {Math.Round(_distance, 0)} km");
      stringBuilder.AppendLine($"- visited countries - {string.Join(",",_visitedCountries)}");

      for (var day = Event.DateFrom; day <= Event.DateTo; day+=1)
      {
        if (_stops.ContainsKey(day))
        {
          var begining = _stops[day].First().Position;
          var end = _stops[day].Last().Position;

          stringBuilder.AppendLine($"## Day {(day - Event.DateFrom).Days + 1} ({day})");

          stringBuilder.AppendLine("Visited places:");
          foreach (var stop in _stops[day])
          {
            stringBuilder.AppendLine(
              $"- {stop.Position.Date:t} - {_geolocation.GetReverseGeolocationData(stop.Position).LocationName }");
          }
        }

        if(media.ContainsKey(day))
        {
          foreach (var medium in media[day])
          {
            stringBuilder.AppendLine($"![{medium.LocationName}](/static/blog/{day.ToString()}/{Path.GetFileName(medium.Path)})");
          }
        }
      }

      fileName.Directory.Create();
      File.WriteAllText(fileName, stringBuilder.ToString());
    }
  }
}