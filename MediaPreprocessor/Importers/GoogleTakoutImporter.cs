using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Handlers.PositionImportHandlers;
using MediaPreprocessor.Importers.GoogleTakeout;
using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaPreprocessor.Importers
{
  class GoogleTakoutImporter : PositionsImporter
  {
    public override bool CanImport(string path)
    {
      return Path.GetExtension(path).ToLower() == ".json";
    }

    protected override IEnumerable<Position> LoadPositions(string trackFile)
    {
      var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
      return records.Locations
        .Where(f => f.Source == "GPS" && f.Accuracy < 150)
        .Select(f => new Position(f.Lat, f.Lng, f.Date));
    }

    public GoogleTakoutImporter(IPositionsRepository positionsRepository, IPositionsImportHandlerFactory handlerFactory,
      ILoggerFactory loggerFactory) : base(positionsRepository, handlerFactory, loggerFactory)
    {
    }
  }
}