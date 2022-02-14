using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Importers
{
  internal class Importers : IImporters
  {
    private readonly IEnumerable<IImporter> _importers;
    private readonly ILogger _log;
    private readonly IInbox _inbox;
    private readonly bool _deleteAfterImport;

    public Importers(IEnumerable<IImporter> importers, IInbox inbox, ILoggerFactory loggerFactory, bool deleteAfterImport)
    {
      _importers = importers;
      _inbox = inbox;
      _deleteAfterImport = deleteAfterImport;
      _log = loggerFactory.CreateLogger<Importers>();
    }

    public void Import()
    {
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
            importer.Import(filePath);

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

      _inbox.Cleanup();
    }
  }
}