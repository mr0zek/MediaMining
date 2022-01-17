using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;

namespace GPSDataProcessor
{
  internal class Position
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
        
    internal class PositionComparer : IEqualityComparer<Position>
    {
      private readonly double _distance;

      public PositionComparer(double distance)
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

    public Position CalculateCenter(IEnumerable<Position> dateAndPositions)
    {
      TimeSpan dateDelta = new TimeSpan();
      double latDelta = 0;
      double lonDelta = 0;

      foreach (var dateAndPosition in dateAndPositions)
      {
        dateDelta = Date - dateAndPosition.Date;
        latDelta = Latitude - dateAndPosition.Latitude;
        lonDelta = Longitude - dateAndPosition.Longitude;
      }

      dateDelta /= dateAndPositions.Count();
      latDelta /= dateAndPositions.Count();
      lonDelta /= dateAndPositions.Count();

      return new Position(Latitude + latDelta, Longitude + lonDelta, Date + dateDelta);
    }    
  }
}