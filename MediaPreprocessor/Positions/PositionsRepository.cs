using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Positions
{
  class PositionsRepository : IPositionsRepository
  {
    private readonly DirectoryPath _basePath;
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

        FilePath filePath = GeneratePositionsPath(p.Key);
        filePath.Directory.Create();
        WriteTrack(_positionsByDate[p.Key], filePath);
      }
    }

    public void WriteTrack(Track track, FilePath filePath)
    {
      var s = JsonConvert.SerializeObject(new FeatureCollection(track.Positions.Select(f => new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(f.Latitude, f.Longitude)),
            new { reportTime = f.Date.ToString("o") })).ToList()), Formatting.Indented);

        File.WriteAllText(filePath, s);      
    }

    private string GeneratePositionsPath(Date date)
    {
      return _basePath
        .AddDirectory(date.Year.ToString())
        .AddDirectory(date.ToString("yyyy-MM"))
        .ToFilePath(date.ToString("yyyy-MM-dd")+".geojson");
    }

    public Track GetFromDay(Date date)
    {
      LoadCoordinatesForDate(date);

      if (_positionsByDate.ContainsKey(date))
      {
        return _positionsByDate[date].Compact();
      }
      else
      {
        return new Track();
      }
    }

    public Track GetFromRange(Date dateFrom, Date dateTo)
    {
      List<Position> result = new List<Position>();

      for (var date = dateFrom; date <= dateTo; date++)
      {
        LoadCoordinatesForDate(date);

        if (_positionsByDate.ContainsKey(date))
        {
          result.AddRange(_positionsByDate[date].Positions);
        }        
      }
      return new Track(result);
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