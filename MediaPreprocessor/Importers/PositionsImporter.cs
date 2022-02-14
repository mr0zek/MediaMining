using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Positions;

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

    public void Import(string filePath)
    {
      var positions = LoadPositions(filePath).ToArray();
      _positionsRepository.AddRange(positions);

      var g = positions.GroupBy(f => f.Date.Date);

      foreach (var handler in _handlers)
      {
        handler.Handle(g.First().Key, g.Last().Key);
      }
    }

    public bool CanImport(string path)
    {
      return Path.GetExtension(path).ToLower() == ".gpx";
    }

    protected abstract IEnumerable<Position> LoadPositions(string trackFile);
  }
}