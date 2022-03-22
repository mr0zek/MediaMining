using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Handlers.ImportHandlers;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Importers
{
  abstract class PositionsImporter : IImporter
  {
    private readonly IPositionsRepository _positionsRepository; 
    private readonly IEnumerable<IPositionsImportHandler> _handlers;

    public PositionsImporter(IPositionsRepository positionsRepository, IPositionsImportHandlerFactory handlersFactory)
    {
      _positionsRepository = positionsRepository;
      _handlers = handlersFactory.Create();
    }

    public ISet<Date> Import(string filePath)
    {
      var positions = LoadPositions(filePath).ToArray();
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