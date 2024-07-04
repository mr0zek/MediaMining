using System;
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
  public class GoogleTakoutImporter : PositionsImporter
  {
    public override bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == "json";
    }

    protected override IEnumerable<Position> LoadPositions(FilePath trackFile)
    {
      var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
      return records.Locations
        .Where(f => f.Source == "GPS" && f.Accuracy < 150)
        .Select(f => new Position(f.Lat, f.Lng, f.Date));
    }  

    public GoogleTakoutImporter(
      IPositionsRepository positionsRepository, 
      IDirectionsProvider directionsPrivider,
      ILoggerFactory loggerFactory,
      DateTime startDate) : base(positionsRepository, directionsPrivider, loggerFactory, startDate)
    {
    }
  }
}