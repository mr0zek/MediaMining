using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Geolocation;
using Newtonsoft.Json;

namespace GoogleTakeoutProcessor
{
  class Program
  {
    static void Main(string[] args)
    {
      EventsRoot events = LoadEvents(@"c:\My\PicturesPrep\Events\events.json");
      DivideByYear(@"c:\My\PicturesPrep\Tracks-All\", @"c:\My\PicturesPrep\Tracks", events);
    }

    private static EventsRoot LoadEvents(string pathToFile)
    {
      return JsonConvert.DeserializeObject<EventsRoot>(File.ReadAllText(pathToFile));
    }

    private static void DivideByYear(string googleTracksPath, string tracksPath, EventsRoot eventsRoot)
    {

      var result = new List<Tuple<DateTime, Coordinate>>();

      var trackFiles = Directory.GetFiles(googleTracksPath, "*.json");

      foreach (var trackFile in trackFiles)
      {
        var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
        result.AddRange(records.Locations.Select(f =>
          new Tuple<DateTime, Coordinate>(f.Date, new Coordinate(f.Lat, f.Lng))));
      }

      var byYear = result.GroupBy(f => f.Item1.Date.Date.Year);
      foreach (var grouping in byYear)
      {
        var groupedByEvent = grouping.GroupBy(f => eventsRoot.GetEventName(f.Item1));
        foreach (var g in groupedByEvent)
        {
          FeatureCollection fc = new FeatureCollection();
          var sortedDates = g.OrderBy(f => f.Item1);
          foreach (var sortedDate in sortedDates)
          {
            fc.Features.Add(
              new Feature(new Point(new Position(sortedDate.Item2.Latitude, sortedDate.Item2.Longitude)),new { reportTime = sortedDate.Item1 }));
          }

          string fileName = Path.Combine(tracksPath,grouping.Key.ToString(),grouping.Key + ".geojson");
          if (g.Key != null)
          {
            fileName = Path.Combine(tracksPath, grouping.Key.ToString(), g.Key+".geojson");
          }

          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
          File.WriteAllText(fileName, JsonConvert.SerializeObject(fc, Formatting.Indented));
        }
      }
      
    }
  }
}
