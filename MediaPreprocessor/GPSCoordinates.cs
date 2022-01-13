using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;

namespace MediaPreprocessor
{
  internal class GPSCoordinates : Dictionary<string, List<Tuple<DateTime, IPosition>>> 
  {
    private readonly IDictionary<string,string> _trackPaths = new Dictionary<string, string>();

    public GPSCoordinates(string tracksPath)
    {
      _trackPaths = Directory.GetFiles(tracksPath, "*.geojson", SearchOption.AllDirectories)
        .ToDictionary(f => Path.GetFileName(f), f => f);
    }

    public void LoadCoordinatesForDate(DateTime date)
    {
      if (this.ContainsKey(date.Date.ToString("yyyy-MM-dd")))
      {
        return;
      }

      var trackFile = _trackPaths[$"{date.Date:yyyy-MM-dd}.geojson"];
      this[date.Date.ToString("yyyy-MM-dd")] = new List<Tuple<DateTime, IPosition>>();

      FeatureCollection features = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(trackFile));

      var positions = features.Features.Select(f =>
        new Tuple<DateTime, IPosition>(DateTime.Parse(f.Properties["reportTime"].ToString()),
          (f.Geometry as Point).Coordinates));

      this[date.Date.ToString("yyyy-MM-dd")].AddRange(positions);
    }

    public IPosition Find(DateTime date)
    {
      try
      {
        var coordinatesFromDay = this[date.Year.ToString()].Where(f => f.Item1.Date == date.Date).OrderBy(f => f.Item1).ToList();
        if (!coordinatesFromDay.Any())
        {
          return null;
        }

        for (int i = 0; i < coordinatesFromDay.Count(); i++)
        {
          if (coordinatesFromDay[i].Item1 > date)
          {
            if (i > 0)
            {
              if (Math.Abs((coordinatesFromDay[i].Item1 - date).TotalSeconds) > Math.Abs((coordinatesFromDay[i - 1].Item1 - date).TotalSeconds))
              {
                return coordinatesFromDay[i - 1].Item2;
              }

              return coordinatesFromDay[i].Item2;
            }

            return coordinatesFromDay[0].Item2;
          }
        }

        return coordinatesFromDay.Last().Item2;
      }
      catch (Exception ex)
      {
        throw new Exception("Error while searching for coordinate", ex);
      }
    }
  }
}