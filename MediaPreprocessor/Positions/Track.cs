using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Positions
{
  public class Track
  {
    public IEnumerable<Position> Positions { get; private set; }
    public DateTime DateFrom => Positions.First().Date;
    public DateTime DateTo => Positions.Last().Date;

    public Track(IEnumerable<Position> positions)
    { 
      Positions = positions.OrderBy(f=>f.Date).ToList();
    }

    public Track() : this(new List<Position>())
    {
    }

    public void WriteAsTrack(FeatureCollection featureCollection, string color)
    {
      Random random = new Random();

      if (Positions.Count() < 2)
      {
        return;
      }
     
      var distance = CalculateDistance();
      if (distance < 1)
      {
        return;
      }


      int pointsPerDay = 2 * (int)distance; // 2 points per km
      int nth = (Positions.Count() / pointsPerDay) + 1;
      var last = Positions.Last();

      var ps2 = Positions;//.Where((x, i) => i % nth == 0 || x == last);
      
      featureCollection.Features.Add(new Feature(
        new LineString(ps2.Select(f=> new GeoJSON.Net.Geometry.Position(f.Latitude, f.Longitude))), 
        new Dictionary<string, object>()
      {        
        {"stroke",color},
        {"stroke-width",5},
        {"stroke-opacity",1},
        {"distance", distance },
        {"dateFrom", Positions.First().Date.ToString("o")},
        {"dateTo", Positions.Last().Date.ToString("o")}
      }));
    }

    public double CalculateDistance()
    {
      if (!Positions.Any())
      {
        return 0;
      }
      double distance = 0;
      Position lastCoordinate = Positions.First();
      foreach (var position in Positions)
      {
        distance += lastCoordinate.DistanceTo(position);
        lastCoordinate = position;
      }

      return distance;
    }

    public static Track Load(FilePath fileName)
    {
      var fc = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(fileName));
      return new Track(fc.Features.Select(f => new Position(
        (f.Geometry as Point).Coordinates.Latitude,
        (f.Geometry as Point).Coordinates.Longitude,
        DateTime.Parse(f.Properties["reportTime"].ToString()))));
    }

    internal void Merge(Track track)
    {
      IEnumerable<Position> x = track.Positions.Except(Positions).ToList();
      Positions = Positions.Concat(x).OrderBy(f=>f.Date);
    }

    public Position FindClosest(DateTime date)
    {
      var coordinatesFromDay = Positions.ToList();
      if (!coordinatesFromDay.Any())
      {
        return null;
      }

      for (int i = 0; i < coordinatesFromDay.Count(); i++)
      {
        if (coordinatesFromDay[i].Date > date)
        {
          if (i > 0)
          {
            return Position.CalculatePositionAtDate(coordinatesFromDay[i],coordinatesFromDay[i - 1], date);
          }

          return coordinatesFromDay[0];
        }
      }

      return coordinatesFromDay.Last();
    }

    public void Write(string filePath)
    {
      var s = JsonConvert.SerializeObject(new FeatureCollection(Positions.Select(f=> new Feature(
        new Point(
          new GeoJSON.Net.Geometry.Position(f.Latitude, f.Longitude)),
          new { reportTime = f.Date.ToString("o") })).ToList()), Formatting.Indented);

      File.WriteAllText(filePath, s);
    }

    public Track Compact()
    {
      List<Tuple<Position, double>> velocity = Positions.SelectWithPrevious((prev, curr) =>
          new Tuple<Position, double>(curr, prev.DistanceTo(curr) / ((curr.Date - prev.Date).TotalSeconds / 60 / 60)))
        .ToList();

      int startFrom = 0;
      for (int i = 0; i < velocity.Count; i++)
      {
        if (velocity[i].Item2 > 1.5)
        {
          startFrom = i;
          break;
        }
      }

      int endTo = 0;
      for (int i = velocity.Count-1; i >= 0; i--)
      {
        if (velocity[i].Item2 >= 1.5)
        {
          break;
        }

        endTo++;
      }

      return new Track(Positions.Skip(startFrom - 1).Take(Positions.Count() - (startFrom - 1) - endTo+1));
    }
  }
}