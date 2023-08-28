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
    private bool _dirty = false;
    private List<Position> _positions = new List<Position>();
    public IEnumerable<Position> Positions
    {
      get
      {
        if (_dirty)
        {
          _positions = _positions.OrderBy(f => f.Date).ToList();
          _dirty = false;
        }
        return _positions;
      }
    }

    public DateTime DateFrom => Positions.First().Date;
    public DateTime DateTo => Positions.Last().Date;

    public Track(IEnumerable<Position> positions)
    { 
      _positions = positions.ToList();
      _dirty = true;
    }

    public Track() : this(new List<Position>())
    {
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
      _positions = _positions.Concat(x).ToList();
      _dirty = true;
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

    internal void Add(Position position)
    {
      _positions = Positions.Concat(new Position[] {position}).OrderBy(f => f.Date).ToList();
      _dirty = true;
    }
  }
}