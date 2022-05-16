using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Handlers.PostImportHandlers;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Importers
{
  internal class Importers : IImporters
  {
    private readonly IEnumerable<IImporter> _importers;
    private readonly ILogger _log;
    private readonly IInbox _inbox;
    private readonly bool _deleteAfterImport;
    private readonly IEnumerable<IPostImportHandler> _postImportHandlers;

    public Importers(IEnumerable<IImporter> importers, IInbox inbox, ILoggerFactory loggerFactory, bool deleteAfterImport, IEnumerable<IPostImportHandler> postImportHandlers)
    {
      _importers = importers;
      _inbox = inbox;
      _deleteAfterImport = deleteAfterImport;
      _postImportHandlers = postImportHandlers;
      _log = loggerFactory.CreateLogger<Importers>();
    }

    public void Import()
    {
      HashSet<Date> changedMediaDate = new HashSet<Date>();

      foreach (var filePath in _inbox.GetFiles())
      {
        IImporter importer = _importers.FirstOrDefault(f => f.CanImport(filePath));
        
        if (importer == null)
        {
          _log.LogError("Unrecognized media file type", filePath);
        }
        else
        {
          try
          {
            _log.LogInformation($"Importing: {filePath}");

            ISet<Date> dates = importer.Import(filePath);
            
            _log.LogInformation($"Imported: {filePath}");

            foreach (var date in dates)
            {
              if (!changedMediaDate.Contains(date))
              {
                changedMediaDate.Add(date);
              }
            }

            if (_deleteAfterImport)
            {
              File.Delete(filePath);
            }
          }
          catch (Exception ex)
          {
            _log.LogError(ex.ToString());
          }
        }
      }

      //if (changedMediaDate.Count > 0)
      //{
      //  foreach (var postImportHandler in _postImportHandlers)
      //  {
      //    postImportHandler.Handle(changedMediaDate);
      //  }
      //}

      _inbox.Cleanup();
    }
  }
}