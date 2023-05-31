using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaMining.PositionImporter
{
  abstract class PositionsImporter : IImporter
  {
    private readonly IPositionsRepository _positionsRepository; 
    private readonly ILogger _log;

    public PositionsImporter(IPositionsRepository positionsRepository,
      ILoggerFactory loggerFactory)
    {
      _positionsRepository = positionsRepository;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Import(FilePath filePath)
    {
      var positions = LoadPositions(filePath).ToArray();
      _log.LogInformation($"Imported {positions.Length} coordinates");

      _positionsRepository.AddRange(positions);

      filePath.DeleteFile();
    }

    public abstract bool CanImport(FilePath path);

    protected abstract IEnumerable<Position> LoadPositions(FilePath trackFile);
  }
}