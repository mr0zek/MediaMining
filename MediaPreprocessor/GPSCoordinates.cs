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
    private readonly string _tracksPath;

    public GPSCoordinates(string tracksPath)
    {
      _tracksPath = tracksPath;
    }

    public void LoadCoordinatesForDate(DateTime date)
    {
      if (this.ContainsKey(date.Year.ToString()))
      {
        return;
      }

      var trackFiles = Directory.GetFiles(Path.Combine(_tracksPath, date.Year.ToString()));
      this[date.Year.ToString()] = new List<Tuple<DateTime, IPosition>>();

      foreach (var trackFile in trackFiles)
      {
        FeatureCollection features = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(trackFile));

        var positions = features.Features.Select(f =>
          new Tuple<DateTime, IPosition>(DateTime.Parse(f.Properties["reportTime"].ToString()),
            (f.Geometry as Point).Coordinates));

        this[date.Year.ToString()].AddRange(positions);
      }
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