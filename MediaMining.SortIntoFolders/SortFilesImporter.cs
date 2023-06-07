using System;
using System.IO;
using System.Linq;
using MediaPreprocessor.Events;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Handlers.MediaImportHandlers;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaMining.SortIntoFolders
{
  class SortFilesImporter : IImporter
  {
    private readonly string[] _knownFileTypes;
    private readonly IMediaTypeDetector _mediaTypeDetector;
    private readonly ExistingDirectoryPath _importToPath;
    private readonly IEventRepository _eventRepository;
    private readonly IGeolocation _geolocation;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger _log;


    public SortFilesImporter(
      IEventRepository eventRepository,
      ILoggerFactory loggerFactory,
      IGeolocation geolocation,
      IPositionsRepository positionsRepository,
      string[] knownFileTypes, 
      IMediaTypeDetector mediaTypeDetector, 
      string importToPath)
    {
      _positionsRepository = positionsRepository;
      _geolocation = geolocation;
      _eventRepository = eventRepository;
      _knownFileTypes = knownFileTypes;
      _mediaTypeDetector = mediaTypeDetector;
      _importToPath = importToPath;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Import(FilePath filePath)
    {
      try
      {
        Media p = Media.FromFile(filePath, MediaId.NewId(), _mediaTypeDetector.Detect(filePath));

        if (p.EventId == null)
        {
          var e = _eventRepository.GetByDate(p.CreatedDate);
          if (e != null)
          {
            p.EventId = e.Id;
            _log.LogInformation($"Event information updated in file: {p.Path} - {e.GetUniqueName()}");
          }
        }

        var position = _positionsRepository.Get(p.CreatedDate);
        if (p.GpsLocation == null && position != null)
        {
          p.GpsLocation = position;
          _log.LogInformation($"GPS information updated in file: {p.Path} - lat:{p.GpsLocation.Latitude}, lon:{p.GpsLocation.Longitude}");
        }
        else
        {
          var distance = p.GpsLocation.DistanceTo(position);
          if (distance > 1) // 1km
          {
            var distances = _positionsRepository.GetFromDay(p.CreatedDate).Positions
              .Where(f => f.Date > p.CreatedDate.AddHours(-2) && f.Date < p.CreatedDate.AddHours(2))
              .Select(f => new Tuple<Position, double>(f, f.DistanceTo(p.GpsLocation)));

            var p2 = distances.OrderBy(f => f.Item2).First();

            _log.LogError($"Distance between calculated and exif position is {distance} km");
          }
        }

        if (p.GpsLocation != null && p.LocationName == null)
        {
          var location = _geolocation.GetReverseGeolocationData(p.GpsLocation);
          p.LocationName = location.LocationName;
          p.Country = location.Country;
          _log.LogInformation($"Location information updated in file: {p.Path} - {p.LocationName}");
        }

        p.MoveTo(CalculateTargetPath(p));
        p.Save();
      }
      catch (Exception ex)
      {
        throw new Exception($"Cannot import {filePath}", ex);
      }
    }

    private FilePath CalculateTargetPath(Media media)
    {
      var targetDirectory = _importToPath.AddDirectory(media.CreatedDate.ToString("yyyy"), media.CreatedDate.ToString("yyyy-MM-dd"));

      if (media.EventId != null)
      {
        Event ex = _eventRepository.Get(media.EventId);
        targetDirectory = _importToPath.AddDirectory(media.CreatedDate.ToString("yyyy"), ex.GetUniqueName(), media.CreatedDate.ToString("yyyy-MM-dd"));        
      }
      
      return targetDirectory.ToFilePath(media.Path.FileName);
    }

    public bool CanImport(FilePath path)
    {
      return _knownFileTypes.Any(f=> f == path.Extension);
    }
  }
}