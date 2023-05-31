  using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    public Importers(IEnumerable<IImporter> importers, IInbox inbox, ILoggerFactory loggerFactory)
    {
      _importers = importers;
      _inbox = inbox;
      _log = loggerFactory.CreateLogger<Importers>();
    }

    public void Import(bool runInParallel)
    {
      IEnumerable<string> files = _inbox.GetFiles();

      _log.LogInformation($"Processing {files.Count()} files");

      int imported = 0;
      if (runInParallel)
      {
        Parallel.ForEach(files, f => Process(f, imported++, files.Count()));
      }
      else
      {
        files.ToList().ForEach(filePath => Process(filePath,imported++,files.Count()));
      }

      _inbox.Cleanup();
    }

    private void Process(string filePath, int i, int count)
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
          _log.LogInformation($"Importing ({i}/{count}): {filePath}");

          importer.Import(filePath);

          _log.LogInformation($"Imported ({i}/{count}): {filePath}");
        }
        catch (Exception ex)
        {
          _log.LogError(ex.ToString());
        }
      }
    }
  }
}