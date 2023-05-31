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

    public void WriteToFile(FilePath fileName)
    {
      var colors = new string[]
      {
        /*Black*/	"#000000",
        /*Red*/ "#FF0000",
        /*salmon*/ "#FA8072",
        /*light salmon*/ "#FFA07A",
        /*Lime*/ "#00FF00",
        /*Blue*/ "#0000FF",
        /*dark khaki*/ "#BDB76B",
        /*Yellow*/ "#FFFF00",
        /*Cyan / Aqua*/ "#00FFFF",	
        /*Magenta / Fuchsia*/ "#FF00FF",	
        /*Silver*/ "#C0C0C0",	
        /*Gray*/ "#808080",	
        /*lime*/ "#00FF00",
        /*lime green*/ "#32CD32",
        /*dodger blue*/ "#1E90FF",
        /*light yellow*/ "#FFFFE0",	
        /*saddle brown*/ "#8B4513",
        /*light blue*/ "#ADD8E6",
        /*sky blue*/ "#87CEEB",
        /*turquoise*/ "#40E0D0",
        /*medium turquoise*/ "#48D1CC",
        /*light green*/ "#90EE90",
        /*Maroon*/ "#800000",
        /*pale golden rod*/	"#EEE8AA",
        /*Olive*/ "#808000",	
        /*Green*/ "#008000",	
        /*Purple*/ "#800080",
        /*Teal*/ "#008080",
        /*Navy"*/ "#000080",
        /*yellow green*/ "#9ACD32",	
        /*dark olive green*/ "#556B2F",	
        /*olive drab*/ "#6B8E23"
      };

      FeatureCollection fc = new FeatureCollection();

      //Tracks
      int colorIndex = 0;
      foreach (var track in _tracksByDay.OrderBy(f => f.Key).Select(f => f.Value))
      {
        track.WriteAsTrack(fc, colors[colorIndex % colors.Length]);
        colorIndex++;
      }

      //Stops
      foreach (var stop in _stops.SelectMany(f => f.Value))
      {
        WriteStop(stop, fc);
      }

      fileName.Directory.Create();

      File.WriteAllText(fileName, JsonConvert.SerializeObject(fc, Formatting.Indented));
    }

    private void WriteStop(Stop stop, FeatureCollection fc)
    {
      var data = _geolocation.GetReverseGeolocationData(stop.Position);
      fc.Features.Add(
        new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(stop.Position.Latitude, stop.Position.Longitude)),
          new Dictionary<string, object>()
          {
            { "marker-color", "#ed1d1d" },
            { "marker-size", "large" },
            { "marker-symbol", "star" },
            { "duration", stop.Duration() },
            { "name", data.LocationName },
            { "date", stop.Position.Date.ToString("o") }
          }));
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