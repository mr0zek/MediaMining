using System;
using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Directions;
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
    protected readonly IDirectionsProvider _directions;
        
    public PositionsImporter(
      IPositionsRepository positionsRepository,
      IDirectionsProvider directions,
      ILoggerFactory loggerFactory)
    {
      _directions = directions;
      _positionsRepository = positionsRepository;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Import(FilePath filePath)
    {
      var positions = LoadPositions(filePath).ToArray();
      _log.LogInformation($"Imported {positions.Length} coordinates");

      positions = LoadFromDirections(positions);

      _positionsRepository.AddRange(positions);

      filePath.DeleteFile();
    }

    public virtual Position[] LoadFromDirections(Position[] positions)
    {
      return positions;
    }

    public abstract bool CanImport(FilePath path);

    protected abstract IEnumerable<Position> LoadPositions(FilePath trackFile);
  }
}