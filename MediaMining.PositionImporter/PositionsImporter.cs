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
    private readonly DateTime _startDate;

    public IPositionsRepository PositionsRepository { get; }
    public IDirectionsProvider Directions { get; }
    public ILoggerFactory LoggerFactory { get; }

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

    protected PositionsImporter(IPositionsRepository positionsRepository, IDirectionsProvider directions, ILoggerFactory loggerFactory)
    {
      PositionsRepository = positionsRepository;
      Directions = directions;
      LoggerFactory = loggerFactory;
    }

    public void Import(FilePath filePath)
    {
      var positions = LoadPositions(filePath).Where(f=>f.Date >= _startDate).ToArray();
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