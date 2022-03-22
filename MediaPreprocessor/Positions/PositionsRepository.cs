using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Positions
{
  class PositionsRepository : IPositionsRepository
  {
    private readonly string _basePath;
    private readonly IDictionary<string, string> _trackPaths;
    private readonly Dictionary<Date, Track> _positionsByDate = new();

    public PositionsRepository(string basePath)
    {
      _basePath = basePath;
      _trackPaths = Directory.GetFiles(basePath, "*.geojson", SearchOption.AllDirectories)
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
      var enumerable = positions as Position[] ?? positions.ToArray();
      var positionsByDate = enumerable.GroupBy(f => new Date(f.Date));
      foreach (var p in positionsByDate)
      {
        LoadCoordinatesForDate(p.Key);
        if (!_positionsByDate.ContainsKey(p.Key))
        {
          _positionsByDate.Add(p.Key, new Track(p));
        }
        else
        {
          _positionsByDate[p.Key].Merge(new Track(p));
        }

        string filePath = GeneratePositionsPath(p.Key);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        _positionsByDate[p.Key].Write(filePath);
      }
    }

    private string GeneratePositionsPath(Date date)
    {
      return Path.Combine(_basePath, date.Year.ToString(), date.ToString("yyyy-MM"),date.ToString("yyyy-MM-dd")+".geojson");
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

    public DateRange GetDateRange()
    {
      var files = Directory.GetFiles(_basePath, "*.geojson", SearchOption.AllDirectories);
      var dates =  files.Select(f => Date.Parse(Path.GetFileNameWithoutExtension(f))).OrderBy(f=>f);
      return new DateRange() {From = dates.First(), To = dates.Last()};
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