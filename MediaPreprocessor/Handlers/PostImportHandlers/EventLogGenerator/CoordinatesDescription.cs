using System;
using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class CoordinatesDescription
  {
    public double Lat { get; }
    public double Lon { get; }

    public CoordinatesDescription(double lat, double lon)
    {
      Lat = lat;
      Lon = lon;
    }

    public double DistanceTo(CoordinatesDescription targetCoordinates)
    {
      if (targetCoordinates == this)
      {
        return 0;
      }
      var baseRad = Math.PI * Lat / 180;
      var targetRad = Math.PI * targetCoordinates.Lat / 180;
      var theta = Lon - targetCoordinates.Lon;
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
  }
}