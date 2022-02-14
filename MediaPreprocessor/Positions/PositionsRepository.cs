using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Positions
{
  class PositionsRepository : IPositionsRepository
  {
    private readonly IDictionary<string, string> _trackPaths;
    private readonly Dictionary<Date, Track> _positionsByDate = new();

    public PositionsRepository(string path)
    {
      _trackPaths = Directory.GetFiles(path, "*.geojson", SearchOption.AllDirectories)
        .ToDictionary(Path.GetFileNameWithoutExtension, f => f);
    }

    public Position Get(DateTime date)
    {
      if (!_positionsByDate.ContainsKey(date))
      {
        LoadCoordinatesForDate(date);
      }

      if (!_positionsByDate.ContainsKey(date))
      {
        return null;
      }

      return _positionsByDate[date].FindClosest(date);
    }

    public void AddRange(IEnumerable<Position> positions)
    {
      var positionsByDate = positions.GroupBy(f => f.Date.Date);
      foreach (var p in positionsByDate)
      {
        LoadCoordinatesForDate(p.Key);
        if (!_positionsByDate.ContainsKey(p.Key))
        {
          _positionsByDate.Add(p.Key, new Track(positions));
        }
        else
        {
          _positionsByDate[p.Key].Merge(new Track(positions));
        }
      }
    }

    public Track GetFromDay(Date date)
    {
      LoadCoordinatesForDate(date);

      if (_positionsByDate.ContainsKey(date))
      {
        return _positionsByDate[date];
      }
      else
      {
        return new Track();
      }
    }

    private void LoadCoordinatesForDate(Date date)
    {
      if (_positionsByDate.ContainsKey(date))
      {
        return;
      }

      if (!_trackPaths.ContainsKey(date.ToString()))
      {
        return;
      }

      var trackFile = _trackPaths[date.ToString()];
      
      _positionsByDate[date] = Track.Load(trackFile);
    }
  }
}