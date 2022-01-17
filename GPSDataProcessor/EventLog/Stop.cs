using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;

namespace GPSDataProcessor
{
  internal class Stop
  {
    public Position Position { get; }

    public Stop(Position position)
    {
      Position = position;
    }

    internal void WriteTo(FeatureCollection fc)
    {
      fc.Features.Add(
        new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(Position.Latitude, Position.Longitude)),
            new Dictionary<string, object>()
      {
        {"marker-color", "#ed1d1d" },
        {"marker-size","large"},
        {"marker-symbol","star"}
      }));
    }

    internal static int ByDateComparer(Stop x, Stop y)
    {
      return x.Position.Date.CompareTo(y.Position.Date);
    }
  }
}