using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Handlers.MediaImportHandlers;
using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Importers
{
  class MediaImporter : IImporter
  {
    private readonly string[] _knownFileTypes;
    private readonly IEnumerable<IMediaImportHandler> _mediaHandlers;
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediaTypeDetector _mediaTypeDetector;

    public MediaImporter(IMediaRepository mediaRepository, string[] knownFileTypes, IMediaImportHandlerFactory mediaHandlersFactory, IMediaTypeDetector mediaTypeDetector)
    {
      _mediaRepository = mediaRepository;
      _knownFileTypes = knownFileTypes;
      _mediaTypeDetector = mediaTypeDetector;
      _mediaHandlers = mediaHandlersFactory.Create();
    }

    public ISet<Date> Import(string filePath)
    {
      try
      {
        Media.Media p = Media.Media.FromFile(filePath, MediaId.NewId(), _mediaTypeDetector.Detect(filePath));
        _mediaRepository.AddToProcess(p); 

        foreach (var handler in _mediaHandlers)
        {
          handler.Handle(p);
        }    

        _mediaRepository.SaveOrUpdate(p);

        return new HashSet<Date>() {p.CreatedDate};
      }
      catch (Exception ex)
      {
        throw new Exception($"Cannot import {filePath}", ex);
      }
    }

    public bool CanImport(string path)
    {
      return _knownFileTypes.Any(f=> f == Path.GetExtension(path).ToLower().Replace(".",""));
    }
  }
}