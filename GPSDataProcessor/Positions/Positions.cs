using Events;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GPSDataProcessor.GoogleTakeout;
using GPSDataProcessor.Gpx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GPSDataProcessor
{
  internal class Positions
  {
    private readonly IDictionary<DateTime, Track> _tracks;

    public Positions(IEnumerable<Position> positions)
    {
      _tracks = positions.GroupBy(f=>f.Date.Date).ToDictionary(f=>f.Key, f=>new Track(f.OrderBy(f=>f.Date)));
    }

    internal static Positions LoadFrom(string tracksPath)
    {
      var result = new List<Position>();

      var trackFiles = Directory.GetFiles(tracksPath, "*.*");

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

      return new Positions(result);
    }

    private static IEnumerable<Position> LoadGpx(string trackFile)
    {
      List<Position> result = new List<Position>();

      using (GpxReader reader = new GpxReader(new FileStream(trackFile, FileMode.Open)))
      {
        while (reader.Read())
        {
          if (reader.ObjectType == GpxObjectType.Track)
          {
            result.AddRange(reader.Track.Segments.SelectMany(f => f.TrackPoints)
              .Select(f => new Position(f.Latitude, f.Longitude, f.Time.Value.ToLocalTime())));
          }
        }
      }

      return result;
    }

    public IDictionary<Event, EventLog> CreateEventLog(EventsRoot eventsRoot)
    {
      Dictionary<Event, EventLog> eventLogs = new Dictionary<Event, EventLog>();

      var groupedByEvent = _tracks.GroupBy(f => eventsRoot.GetEvent(f.Key));
      foreach (var g in groupedByEvent)
      {
        if (g.Key == null)
        {
          continue;
        }
        eventLogs.Add(g.Key, new EventLog(g.Key));
        foreach (KeyValuePair<DateTime, Track> day in g)
        {
          eventLogs[g.Key].AddStops(day.Value.DetectStops());
          eventLogs[g.Key].AddDayTrack(day.Key, day.Value);
        }
      }
      return eventLogs;
    }    

    internal void WriteToDirectory(string outputDirectory, EventsRoot eventsRoot)
    {
      foreach (var track in _tracks)
      {
        var e = eventsRoot.GetEvent(track.Key);
        string fileName = Path.Combine(
          outputDirectory, 
          track.Key.Year.ToString(), 
          track.Key.ToString("yyyy-MM"),
          track.Key.ToString("yyyy-MM-dd") + ".geojson");

        if (e != null)
        {
          fileName = Path.Combine(
            outputDirectory, 
            track.Key.Year.ToString(), 
            e.GetUniqueName(),
            track.Key.Date.ToString("yyyy-MM-dd") + ".geojson");
        }
        track.Value.WriteAsPoints(fileName);
      }      
    }

    private static IEnumerable<Position> LoadFromGoogleTakeout(string trackFile)
    {
      var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
      return records.Locations
        .Where(f => f.Source == "GPS" && f.Accuracy < 150)
        .Select(f => new Position(f.Lat, f.Lng, f.Date));
    }

    public void ExportDailyDistanceStats(string fileName)
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendLine("Date;Distance");
      foreach (var day in _tracks.OrderBy(f => f.Key))
      {
        var distance = day.Value.CalculateDistance();
        builder.AppendLine($"{day.Key:yyyy-MM-dd};{distance}");
      }

      File.WriteAllText(fileName, builder.ToString());
    }
  }
}