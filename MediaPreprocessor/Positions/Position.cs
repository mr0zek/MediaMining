using System;
using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Positions
{
  public class Position : ValueObject
  {
    public DateTime Date { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public Position(double latitude, double longitude, DateTime date)
    {
      Latitude = latitude;
      Longitude = longitude;
      Date = date;
    }

    public override string ToString()
    {
      return $"Lat:{Latitude}, Lon:{Longitude}, Date:{Date}";
    }

    internal class PositionDistanceComparer : IEqualityComparer<Position>
    {
      private readonly double _distance;

      public PositionDistanceComparer(double distance)
      {
        _distance = distance;
      }

      public bool Equals(Position x, Position y)
      {
        var distance =  x.DistanceTo(y);
        return distance < _distance;
      }

      public int GetHashCode(Position obj)
      {
        return 1;
      }
    }

    public double DistanceTo(Position targetCoordinates)
    {
      if (targetCoordinates == this)
      {
        return 0;
      }
      var baseRad = Math.PI * Latitude / 180;
      var targetRad = Math.PI * targetCoordinates.Latitude / 180;
      var theta = Longitude - targetCoordinates.Longitude;
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

      return UnitOfLength.Kilometers.ConvertFromMiles(dist);
    }

    public static Position CalculateCenter(IEnumerable<Position> positions)
    {
      positions = positions.OrderBy(f => f.Date);

      double lat = 0;
      double lon = 0;

      foreach (var position in positions)
      {
        lat += position.Latitude;
        lon += position.Longitude;
      }

      lat /= positions.Count();
      lon /= positions.Count();

      return new Position(lat, lon, positions.First().Date + (positions.Last().Date - positions.First().Date)/2);
    }

    protected override object[] GetValues()
    {
      return new object[] {Latitude, Longitude};
    }
  }
}