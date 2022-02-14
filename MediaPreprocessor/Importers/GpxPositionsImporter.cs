using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Importers.Gpx;
using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Importers
{
  class GpxPositionsImporter : PositionsImporter
  {
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


    public GpxPositionsImporter(IPositionsRepository positionsRepository, IPositionsImportHandlerFactory handlerFactory) : base(positionsRepository, handlerFactory)
    {
    }
  }
}