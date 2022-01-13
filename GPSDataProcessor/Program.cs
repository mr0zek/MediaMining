using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Events;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GPSDataProcessor.GoogleTakeout;
using GPSDataProcessor.Gpx;
using Newtonsoft.Json;

namespace GPSDataProcessor
{
  class Program
  {
    static void Main(string[] args)
    {
      EventsRoot events = EventsRoot.LoadFromPath(@"c:\My\PicturesPrep\Events\");
      DivideByYear(@"c:\My\PicturesPrep\Tracks-All\", @"c:\My\PicturesPrep\Tracks", events);
    }

    private static void DivideByYear(string googleTracksPath, string tracksPath, EventsRoot eventsRoot)
    {

      var result = new List<DateAndPosition>();

      var trackFiles = Directory.GetFiles(googleTracksPath, "*.*");

      foreach (var trackFile in trackFiles)
      {
        if (Path.GetExtension(trackFile).ToLower() == ".gpx")
        {
          result.AddRange(LoadGpx(trackFile));
          Console.WriteLine("GPX loaded : " + trackFile);
        }

        if (Path.GetExtension(trackFile).ToLower() == ".json") //google takeout
        {
          result.AddRange(LoadFromGoogleTakeout(trackFile));
          Console.WriteLine("GoogleTakeout loaded : " + trackFile);
        }
      }

      Dictionary<string, EventLog> eventLogs = new Dictionary<string, EventLog>();
      foreach (Event @event in eventsRoot.Events)
      {
        Directory.CreateDirectory(
          Path.Combine(tracksPath, @event.DateFrom.Date.Year.ToString(), @event.GetUniqueName()));
        eventLogs.Add(@event.GetUniqueName(), new EventLog(@event.GetUniqueName(), @event.DateFrom, @event.DateTo));
      }

      //result = result.Where(f=>f.Date.Date == new DateTime(2020,6,23)).ToList();

      var groupedByEvent = result.GroupBy(f => eventsRoot.GetEvent(f.Date));
      foreach (var g in groupedByEvent)
      {
        if (g.Key != null)
        {
          var grouppedByDay = g.GroupBy(f => f.Date.Date);
          foreach (var day in grouppedByDay)
          {
            FeatureCollection fc = new FeatureCollection();
            string fileName = Path.Combine(tracksPath, day.Key.Year.ToString(), g.Key.GetUniqueName(),
              day.Key.Date.ToString("yyyy-MM-dd") + ".geojson");

            foreach (var sortedDate in day.OrderBy(f => f.Date))
            {
              fc.Features.Add(
                new Feature(new Point(new Position(sortedDate.Position.Latitude, sortedDate.Position.Longitude)),
                  new {reportTime = sortedDate.Date}));
            }

            eventLogs[g.Key.GetUniqueName()].AddStops(DetectStops(day));
            eventLogs[g.Key.GetUniqueName()].AddDayRoute(day);

            File.WriteAllText(fileName, JsonConvert.SerializeObject(fc, Formatting.Indented));
          }
        }
        else
        {
          var grouppedByDay = g.GroupBy(f => f.Date.Date);
          foreach (var day in grouppedByDay)
          {
            var sortedDates = day.OrderBy(f => f.Date);
            FeatureCollection fc = new FeatureCollection();
            foreach (var sortedDate in sortedDates)
            {
              fc.Features.Add(
                new Feature(new Point(new Position(sortedDate.Position.Latitude, sortedDate.Position.Longitude)),
                  new {reportTime = sortedDate.Date}));
            }

            string fileName = Path.Combine(tracksPath, day.Key.Year.ToString(), day.Key.ToString("yyyy-MM"),
              day.Key.ToString("yyyy-MM-dd") + ".geojson");

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllText(fileName, JsonConvert.SerializeObject(fc, Formatting.Indented));
          }
        }
      }

      foreach (var eventLog in eventLogs.Values)
      {
        File.WriteAllText(
          Path.Combine(tracksPath, eventLog.DateFrom.Year.ToString(), eventLog.Name, eventLog.Name+"_eventLog.geojson"),
          JsonConvert.SerializeObject(eventLog.FeatureCollection, Formatting.Indented));
      }

      Console.WriteLine("Calculating distance ...");

      var distanceByDay = CalculateDistance(result);
      SaveDistancecByDay(Path.Combine(tracksPath, "distance_stats.csv"), distanceByDay);
    }

    private static IEnumerable<DateAndPosition> DetectStops(IEnumerable<DateAndPosition> day)
    {
      var velocity = day.Skip(1).SelectWithPrevious((prev, curr) =>
        new Tuple<DateAndPosition, double>(curr, prev.Position.DistanceTo(curr.Position) / ((curr.Date - prev.Date).TotalSeconds / 60 / 60))).ToList();

      var r2 = velocity.Where(f => f.Item2 < 5).Select(f => f.Item1).ToList();

      if (r2.Count == 0)
      {
        return new List<DateAndPosition>();
      }

      List<List<DateAndPosition>> result = new List<List<DateAndPosition>>();

      DateAndPosition key = r2.First();
      result.Add(new List<DateAndPosition>(){ key });
      foreach (var dateAndPosition in r2.Skip(1))
      {
        var p1 = result.Last().First();
        if (p1.Position.DistanceTo(dateAndPosition.Position) < 0.1)
        {
          result.Last().Add(dateAndPosition);
        }
        else
        {
          result.Add(new List<DateAndPosition>(){ dateAndPosition });
        }
      }

      result = result.Where(f => (f.Last().Date - f.First().Date).TotalMinutes > 10).ToList();
      
      var r4 = result.Select(f => f.First().CalculateCenter(f));

      var r5 = r4.Distinct(new DateAndPosition.PositionComparer(0.2)).ToList();
        
      return r5;
    }
    
    private static void SaveDistancecByDay(string fileName, Dictionary<DateTime, double> distanceByDay)
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendLine("Date;Distance");
      foreach (var day in distanceByDay.OrderBy(f=>f.Key))
      {
        builder.AppendLine($"{day.Key:yyyy-MM-dd};{day.Value}");
      }

      File.WriteAllText(fileName, builder.ToString());
    }

    private static Dictionary<DateTime, double> CalculateDistance(List<DateAndPosition> list)
    {
      Dictionary<DateTime, double> distanceByDay = new Dictionary<DateTime, double>();

      var byDate = list.GroupBy(f => f.Date.Date);
      foreach (var grouping in byDate)
      {
        distanceByDay.Add(grouping.Key, EventLog.CalculateDistance(grouping));
        Console.WriteLine("Day calculated : "+grouping.Key);
      }

      return distanceByDay;
    }

    private static IEnumerable<DateAndPosition> LoadGpx(string trackFile)
    {
      List<DateAndPosition> result = new List<DateAndPosition>();

      using (GpxReader reader = new GpxReader(new FileStream(trackFile, FileMode.Open)))
      {
        while (reader.Read())
        {
          if (reader.ObjectType == GpxObjectType.Track)
          {
            result.AddRange(reader.Track.Segments.SelectMany(f => f.TrackPoints)
              .Select(f => new DateAndPosition(f.Time.Value.ToLocalTime(), new Position(f.Latitude, f.Longitude))));
          }
        }
      }

      return result;
    }

    private static IEnumerable<DateAndPosition> LoadFromGoogleTakeout(string trackFile)
    {
      var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
      return records.Locations.Where(f=>f.Source == "GPS" && f.Accuracy < 150).Select(f =>
        new DateAndPosition(f.Date, new Position(f.Lat, f.Lng))).ToList();
    }
  }
}


