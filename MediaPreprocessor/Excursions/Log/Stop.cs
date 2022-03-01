using System;
using System.Collections.Generic;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Position = MediaPreprocessor.Positions.Position;

namespace MediaPreprocessor.Excursions.Log
{
  public class Stop
  {
    public string Name { get; }
    public Position Position { get; }
    public TimeSpan Duration { get; }

    public Stop(string name, Position position, TimeSpan duration)
    {
      Name = name;
      Position = position;
      Duration = duration;
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
        {"marker-symbol","star"},
        {"duration", Duration },
        {"name", Name },
        {"date", Position.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}
      }));
    }
  }
}