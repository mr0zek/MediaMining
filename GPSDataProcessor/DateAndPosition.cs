using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;

namespace GPSDataProcessor
{
  internal class DateAndPosition
  {
    public DateAndPosition(DateTime date, Position position)
    {
      Date = date;
      Position = position;
    }

    public DateTime Date { get; set; }
    public Position Position { get; set; }

    internal class PositionComparer : IEqualityComparer<DateAndPosition>
    {
      private readonly double _distance;

      public PositionComparer(double distance)
      {
        _distance = distance;
      }

      public bool Equals(DateAndPosition x, DateAndPosition y)
      {
        var distance =  x.Position.DistanceTo(y.Position);
        return distance < _distance;
      }

      public int GetHashCode(DateAndPosition obj)
      {
        return 1;
      }
    }

    public DateAndPosition CalculateCenter(IEnumerable<DateAndPosition> dateAndPositions)
    {
      TimeSpan dateDelta = new TimeSpan();
      double latDelta = 0;
      double lonDelta = 0;

      foreach (var dateAndPosition in dateAndPositions)
      {
        dateDelta = Date - dateAndPosition.Date;
        latDelta = Position.Latitude - dateAndPosition.Position.Latitude;
        lonDelta = Position.Longitude - dateAndPosition.Position.Longitude;
      }

      dateDelta /= dateAndPositions.Count();
      latDelta /= dateAndPositions.Count();
      lonDelta /= dateAndPositions.Count();

      return new DateAndPosition(Date + dateDelta,
        new Position(Position.Latitude + latDelta, Position.Longitude + lonDelta));
    }
  }
}