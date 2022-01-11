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
using GPSDataProcessor.Gpx;
using Newtonsoft.Json;

namespace GPSDataProcessor
{
  class Program
  {
    static void Main(string[] args)
    {
      ReverseGeolocationResponse UpdateLocationName(string lat, string lon)
      {
        HttpClient httpClient = new HttpClient();
        var s = "text/html,application/xhtml+xml,application/xml,image/avif,image/webp,image/apng,*/*,application/signed-exchange";
        foreach (var s1 in s.Split(","))
        {
          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(s1));
        }

        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "97.0.4692.71"));

        var task = httpClient.GetAsync($"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lon}&zoom=18&addressdetails=1");
        task.Wait();
        var t2 = task.Result.Content.ReadAsStringAsync();
          t2.Wait();
        return JsonConvert.DeserializeObject<ReverseGeolocationResponse>(t2.Result);
      }

      ReverseGeolocationResponse s = UpdateLocationName("51.1601582", "17.1114598");

      EventsRoot events = EventsRoot.LoadFromPath(@"c:\My\PicturesPrep\Events\");
      DivideByYear(@"c:\My\PicturesPrep\Tracks-All\", @"c:\My\PicturesPrep\Tracks", events);
    }

    private static void DivideByYear(string googleTracksPath, string tracksPath, EventsRoot eventsRoot)
    {

      var result = new List<Tuple<DateTime, Position>>();

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
          Console.WriteLine("GoogleTakeout loaded : "+trackFile);
        }
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
              new Feature(new Point(new Position(sortedDate.Item2.Latitude, sortedDate.Item2.Longitude)),
                new {reportTime = sortedDate.Item1}));
          }

          string fileName = Path.Combine(tracksPath, grouping.Key.ToString(), grouping.Key + ".geojson");
          if (g.Key != null)
          {
            fileName = Path.Combine(tracksPath, grouping.Key.ToString(), g.Key + ".geojson");
          }

          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
          File.WriteAllText(fileName, JsonConvert.SerializeObject(fc, Formatting.Indented));
        }
      }

      Console.WriteLine("Calculating distance ...");

      var distanceByDay = CalculateDistance(result);
      SaveDistancecByDay(Path.Combine(tracksPath,"distance_stats.csv"), distanceByDay);
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

    private static Dictionary<DateTime, double> CalculateDistance(List<Tuple<DateTime, Position>> list)
    {
      Dictionary<DateTime, double> distanceByDay = new Dictionary<DateTime, double>();

      var byDate = list.GroupBy(f => f.Item1.Date.Date);
      foreach (var grouping in byDate)
      {
        double distance = 0;
        var sortedGrouping = grouping.OrderBy(f => f.Item1).ToArray();
        Tuple<DateTime, Position> lastCoordinate = sortedGrouping.First();
        foreach (Tuple<DateTime, Position> t in sortedGrouping)
        {
          //var s = string.Join(Environment.NewLine, (sortedGrouping).Select(f => "{\"type\": \"Feature\",\"properties\": {\"reportTime\" : \"" + f.Item1.ToString("yyyy-MM-dd HH:mm:ss") + "\"},\"geometry\": { \"type\": \"Point\",\"coordinates\": [" + f.Item2.Longitude.ToString().Replace(",", ".") + "," + f.Item2.Latitude.ToString().Replace(",", ".") + "]}},"));
          //var s2 = string.Join(Environment.NewLine, (new []{ lastCoordinate, t}).Select(f => "{\"type\": \"Feature\",\"properties\": {\"reportTime\" : \"" + f.Item1.ToString("yyyy-MM-dd hh:mm:ss") + "\"},\"geometry\": { \"type\": \"Point\",\"coordinates\": [" + f.Item2.Longitude.ToString().Replace(",", ".") + "," + f.Item2.Latitude.ToString().Replace(",", ".") + "]}},"));
          //Console.WriteLine(s);

          distance += lastCoordinate.Item2.DistanceTo(t.Item2);
          lastCoordinate = t;
        }

        distanceByDay.Add(grouping.Key, distance);
        Console.WriteLine("Day calculated : "+grouping.Key);
      }

      return distanceByDay;
    }

    private static IEnumerable<Tuple<DateTime, Position>> LoadGpx(string trackFile)
    {
      List<Tuple<DateTime, Position>> result = new List<Tuple<DateTime, Position>>();

      using (GpxReader reader = new GpxReader(new FileStream(trackFile, FileMode.Open)))
      {
        while (reader.Read())
        {
          if (reader.ObjectType == GpxObjectType.Track)
          {
            result.AddRange(reader.Track.Segments.SelectMany(f => f.TrackPoints)
              .Select(f => new Tuple<DateTime, Position>(f.Time.Value, new Position(f.Latitude, f.Longitude))));
          }
        }
      }

      return result;
    }

    private static IEnumerable<Tuple<DateTime, Position>> LoadFromGoogleTakeout(string trackFile)
    {
      var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
      return records.Locations.Where(f=>f.Source == "GPS").Select(f =>
        new Tuple<DateTime, Position>(f.Date, new Position(f.Lat, f.Lng))).ToList();
    }
  }

  public static class CoordinatesDistanceExtensions
  {
    public static double DistanceTo(this Position baseCoordinates, Position targetCoordinates)
    {
      return DistanceTo(baseCoordinates, targetCoordinates, UnitOfLength.Kilometers);
    }

    public static double DistanceTo(this Position baseCoordinates, Position targetCoordinates,
      UnitOfLength unitOfLength)
    {
      if (baseCoordinates == targetCoordinates)
      {
        return 0;
      }
      var baseRad = Math.PI * baseCoordinates.Latitude / 180;
      var targetRad = Math.PI * targetCoordinates.Latitude / 180;
      var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
      var thetaRad = Math.PI * theta / 180;

      double dist =
        Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
        Math.Cos(targetRad) * Math.Cos(thetaRad);
      if (dist > 1)
      {
        dist = 1;
      }
      dist = Math.Acos(dist);

      dist = dist * 180 / Math.PI;
      dist = dist * 60 * 1.1515;

      return unitOfLength.ConvertFromMiles(dist);
    }
  }

  public class UnitOfLength
  {
    public static UnitOfLength Kilometers = new UnitOfLength(1.609344);
    public static UnitOfLength NauticalMiles = new UnitOfLength(0.8684);
    public static UnitOfLength Miles = new UnitOfLength(1);

    private readonly double _fromMilesFactor;

    private UnitOfLength(double fromMilesFactor)
    {
      _fromMilesFactor = fromMilesFactor;
    }

    public double ConvertFromMiles(double input)
    {
      return input * _fromMilesFactor;
    }
  }
}


