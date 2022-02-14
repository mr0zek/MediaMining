using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MediaPreprocessor.Excursions.Log;
using MediaPreprocessor.Geolocation;
using Newtonsoft.Json;

namespace MediaPreprocessor.Positions
{
  public class Track
  {
    private IEnumerable<Position> _positions;

    public Track(IEnumerable<Position> positions)
    { 
      _positions = positions.OrderBy(f=>f.Date).ToList();
    }

    public Track() : this(new List<Position>())
    {
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
        {"stroke-opacity",1},
        {"distance", distance }
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

    public IEnumerable<Stop> DetectStops(IGeolocation geolocation, IStopDetection stopDetection)
    {
      var stopCandidates = stopDetection.Detect(_positions);

      var stops = stopCandidates.Select(f =>
      {
        var name = geolocation.GetReverseGeolocationData(f.Item1).GetLocationName();
        return new Stop(name, f.Item1, f.Item2);
      }).ToList();
      
      return stops;
    }

    internal static Track Load(string fileName)
    {
      var fc = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(fileName));
      return new Track(fc.Features.Select(f => new Position(
        (f.Geometry as Point).Coordinates.Latitude,
        (f.Geometry as Point).Coordinates.Longitude,
        DateTime.Parse(f.Properties["reportTime"].ToString()))));
    }

    internal void Merge(Track track)
    {
      IEnumerable<Position> x = track._positions.Except(_positions).ToList();
      _positions = _positions.Concat(x).OrderBy(f=>f.Date);
    }

    public Position FindClosest(DateTime date)
    {
      var coordinatesFromDay = _positions.ToList();
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
            if (Math.Abs((coordinatesFromDay[i].Date - date).TotalSeconds) > Math.Abs((coordinatesFromDay[i - 1].Date - date).TotalSeconds))
            {
              return coordinatesFromDay[i - 1];
            }

            return coordinatesFromDay[i];
          }

          return coordinatesFromDay[0];
        }
      }

      return coordinatesFromDay.Last();
    }
  }
}