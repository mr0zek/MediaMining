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
  public abstract class PositionsImporter : IImporter
  {
    private readonly IPositionsRepository _positionsRepository; 
    private readonly ILogger _log;
    protected readonly IDirectionsProvider _directions;
    private readonly DateTime _startDate;

    public PositionsImporter(
      IPositionsRepository positionsRepository,
      IDirectionsProvider directions,
      ILoggerFactory loggerFactory,
      DateTime startDate)
    {
      _directions = directions;
      _startDate = startDate;
      _positionsRepository = positionsRepository;
      _log = loggerFactory.CreateLogger(GetType());
    }    

    public void Import(FilePath filePath)
    {
      var positions = LoadPositions(filePath).Where(f=>f.Date >= _startDate).ToArray();
      _log.LogInformation($"Imported {positions.Length} coordinates");

      positions = LoadFromDirections(positions);

      filePath.DeleteFile();

      _positionsRepository.AddRange(positions);      
    }

    public Position[] LoadFromDirections(Position[] positions)
    {
      List<Position> result = new List<Position>(positions);
      Position prevPosition = positions.First();
      foreach (var position in positions)
      {
        if (prevPosition.DistanceTo(position) > 1)
        {
          result.AddRange(_directions.GetDirections(prevPosition, position).Positions);
        }
        prevPosition = position;
      }

      return result.ToArray();
    }

    public abstract bool CanImport(FilePath path);

    protected abstract IEnumerable<Position> LoadPositions(FilePath trackFile);
  }
}