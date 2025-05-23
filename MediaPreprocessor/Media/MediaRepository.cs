﻿using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaPreprocessor.Media
{
  public class MediaRepository : IMediaRepository
  {
    private class MediaDescription
    {
      public FilePath Path { get; internal set; }
      public DateTime Date { get; internal set; }
      public string Id { get; internal set; }
    }
      
    private readonly IMediaTypeDetector _mediaTypeDetector;
    private IDictionary<Date, List<MediaDescription>> _media;
    private ILogger _log;

    public MediaRepository(string basePath, IMediaTypeDetector mediaTypeDetector, ILoggerFactory loggerFactory)
    {
      _log = loggerFactory.CreateLogger<MediaRepository>();
      _mediaTypeDetector = mediaTypeDetector;
      int id = 1;
      _media = Directory
        .GetFiles(basePath, "*.*", SearchOption.AllDirectories)
        .Select(f => FilePath.Parse(f))
        .Where(x => _mediaTypeDetector.IsKnownType(x))
        .Select(f=> new MediaDescription() { Date = GetDateFromPath(f), Path = f, Id = (id++).ToString() })
        .GroupBy(f=> (Date)f.Date)
        .ToDictionary(k=>k.Key, v=>v.ToList());      
    }

    private DateTime GetDateFromPath(string path)
    {
      var result = Regex.Match(path, @"(?<date>\d\d\d\d-\d\d-\d\d)");
      return DateTime.Parse(result.Groups["date"].ToString());
    }

    public IEnumerable<Media> GetAll(Date eventDateFrom, Date eventDateTo)
    {
      List<Media> result = new List<Media>();
      for (Date date = eventDateFrom; date <= eventDateTo; date++)
      {
        if (_media.ContainsKey(date))
        {
          result.AddRange(_media[date].AsParallel().Select(f =>
          {
            _log.LogInformation($"Loading media: {f.Path}");
            return Media.FromFile(f.Path, new MediaId(f.Id), _mediaTypeDetector.Detect(f.Path));
          }));
        }
      }
      return result;
    }    
  }
}