using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Handlers.ImportHandlers;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Importers
{
  abstract class PositionsImporter : IImporter
  {
    private readonly IPositionsRepository _positionsRepository; 
    private readonly IEnumerable<IPositionsImportHandler> _handlers;
    private readonly ILogger _log;

    public PositionsImporter(IPositionsRepository positionsRepository, IPositionsImportHandlerFactory handlersFactory,
      ILoggerFactory loggerFactory)
    {
      _positionsRepository = positionsRepository;
      _handlers = handlersFactory.Create();
      _log = loggerFactory.CreateLogger(GetType());
    }

    public ISet<Date> Import(string filePath)
    {
      var positions = LoadPositions(filePath).ToArray();
      _log.LogInformation($"Imported {positions.Length} coordinates");

      _positionsRepository.AddRange(positions);

      var g = positions.GroupBy(f => new Date(f.Date));

      foreach (var handler in _handlers)
      {
        handler.Handle(g.First().Key, g.Last().Key);
      }

      return g.Select(f => f.Key).ToHashSet();
    }

    public abstract bool CanImport(string path);

    protected abstract IEnumerable<Position> LoadPositions(string trackFile);
  }
}