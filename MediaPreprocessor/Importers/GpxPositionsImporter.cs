using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Handlers.ImportHandlers;
using MediaPreprocessor.Importers.Gpx;
using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Importers
{
  class GpxPositionsImporter : PositionsImporter
  {
    public override bool CanImport(string path)
    {
      return Path.GetExtension(path).ToLower() == ".gpx";
    }

    protected override IEnumerable<Position> LoadPositions(string trackFile)
    {
      List<Position> result = new List<Position>();

      using (GpxReader reader = new GpxReader(new FileStream(trackFile, FileMode.Open)))
      {
        while (reader.Read())
        {
          if (reader.ObjectType == GpxObjectType.Track)
          {
            result.AddRange(reader.Track.Segments.SelectMany(f => f.TrackPoints)
              .Select(f => new Position(f.Latitude, f.Longitude, f.Time.Value.ToLocalTime())));
          }
        }
      }

      return result;
    }


    public GpxPositionsImporter(IPositionsRepository positionsRepository, IPositionsImportHandlerFactory handlerFactory,
      ILoggerFactory loggerFactory) : base(positionsRepository, handlerFactory, loggerFactory)
    {
    }
  }
}