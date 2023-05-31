using System;
using System.IO;
using System.Linq;
using MediaPreprocessor.Events;
using MediaPreprocessor.Handlers.MediaImportHandlers;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Media;
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
    private readonly ILogger _log;


    public SortFilesImporter(
      IEventRepository eventRepository,
      ILoggerFactory loggerFactory,
      string[] knownFileTypes, 
      IMediaTypeDetector mediaTypeDetector, 
      string importToPath)
    {
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

        p.MoveTo(CalculateTargetPath(p));
        p.Save();
      }
      catch (Exception ex)
      {
        throw new Exception($"Cannot import {filePath}", ex);
      }
    }

    private FilePath CalculateTargetPath(MediaPreprocessor.Media.Media media)
    {
      var targetDirectory = _importToPath.AddDirectory(media.CreatedDate.ToString("yyyy"), media.CreatedDate.ToString("yyyy-MM-dd"));

      if (media.EventId != null)
      {
        Event ex = _eventRepository.Get(media.EventId);
        targetDirectory = _importToPath.AddDirectory(media.CreatedDate.ToString("yyyy"), ex.GetUniqueName(),
          media.CreatedDate.ToString("yyyy-MM-dd"));
      }

      return targetDirectory.ToFilePath(media.Path.FileName);
    }

    public bool CanImport(FilePath path)
    {
      return _knownFileTypes.Any(f=> f == path.Extension.ToLower().Replace(".",""));
    }
  }
}