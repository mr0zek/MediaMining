using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaMining.PositionImporter.GoogleTakeout;
using MediaPreprocessor.Directions;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaMining.PositionImporter
{
  class GoogleTakoutImporter : PositionsImporter
  {
    public override bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == ".json";
    }

    protected override IEnumerable<Position> LoadPositions(FilePath trackFile)
    {
      var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
      return records.Locations
        .Where(f => f.Source == "GPS" && f.Accuracy < 150)
        .Select(f => new Position(f.Lat, f.Lng, f.Date));
    }

    public override Position[] LoadFromDirections(Position[] positions)
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

    public GoogleTakoutImporter(
      IPositionsRepository positionsRepository, 
      IDirectionsProvider directionsPrivider,
      ILoggerFactory loggerFactory) : base(positionsRepository, directionsPrivider, loggerFactory)
    {
    }
  }
}