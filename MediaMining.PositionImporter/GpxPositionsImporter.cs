using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaMining.PositionImporter.Gpx;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaMining.PositionImporter
{
  class GpxPositionsImporter : PositionsImporter
  {
    public override bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == ".gpx";
    }

    protected override IEnumerable<Position> LoadPositions(FilePath trackFile)
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


    public GpxPositionsImporter(IPositionsRepository positionsRepository, 
      ILoggerFactory loggerFactory) : base(positionsRepository, loggerFactory)
    {
    }
  }
}