using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPSDataProcessor
{
  internal class Track
  {
    private IEnumerable<Position> _positions;

    public Track(IEnumerable<Position> positions)
    {
      _positions = positions;
    }

    public void WriteAsPoints(string fileName)
    {
      FeatureCollection featureCollection = new FeatureCollection();

      foreach (var position in _positions)
      {
        featureCollection.Features.Add(new Feature(
          new Point(new GeoJSON.Net.Geometry.Position(position.Latitude, position.Longitude)), 
          new Dictionary<string, object>()
          {
            {"reportTime",position.Date }
          }));
      }

      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      File.WriteAllText(fileName, JsonConvert.SerializeObject(featureCollection, Formatting.Indented));
    }
    public void WriteAsTrack(FeatureCollection featureCollection, string color)
    {
      Random random = new Random();

      if (_positions.Count() < 2)
      {
        return;
      }
     
      var distance = CalculateDistance();
      if (distance < 1)
      {
        return;
      }


      int pointsPerDay = 2 * (int)distance; // 2 points per km
      int nth = (_positions.Count() / pointsPerDay) + 1;
      var last = _positions.Last();

      var ps2 = _positions.Where((x, i) => i % nth == 0 || x == last);
      
      featureCollection.Features.Add(new Feature(
        new LineString(ps2.Select(f=> new GeoJSON.Net.Geometry.Position(f.Latitude, f.Longitude))), 
        new Dictionary<string, object>()
      {        
        {"stroke",color},
        {"stroke-width",5},
        {"stroke-opacity",1}
      }));
    }

    public double CalculateDistance()
    { 
      double distance = 0;
      Position lastCoordinate = _positions.First();
      foreach (var position in _positions)
      {
        distance += lastCoordinate.DistanceTo(position);
        lastCoordinate = position;
      }

      return distance;
    }

    public IEnumerable<Position> DetectStops()
    {      
      var velocity = _positions.Skip(1).SelectWithPrevious((prev, curr) =>
        new Tuple<Position, double>(curr, prev.DistanceTo(curr) / ((curr.Date - prev.Date).TotalSeconds / 60 / 60))).ToList();

      var r2 = velocity.Where(f => f.Item2 < 5).Select(f => f.Item1).ToList();

      if (r2.Count == 0)
      {
        return new List<Position>();
      }

      List<List<Position>> result = new List<List<Position>>();

      Position key = r2.First();
      result.Add(new List<Position>() { key });
      foreach (var dateAndPosition in r2.Skip(1))
      {
        var p1 = result.Last().First();
        if (p1.DistanceTo(dateAndPosition) < 0.1)
        {
          result.Last().Add(dateAndPosition);
        }
        else
        {
          result.Add(new List<Position>() { dateAndPosition });
        }
      }

      result = result.Where(f => (f.Last().Date - f.First().Date).TotalMinutes > 10).ToList();

      var r4 = result.Select(f => f.First().CalculateCenter(f));

      var r5 = r4.Distinct(new Position.PositionComparer(0.2)).ToList();

      return r5;
    }
  }
}