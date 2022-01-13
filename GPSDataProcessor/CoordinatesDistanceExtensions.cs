using System;
using GeoJSON.Net.Geometry;

namespace GPSDataProcessor
{
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
}