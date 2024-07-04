using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaMining.PositionImporter.Gpx;
using MediaPreprocessor.Directions;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaMining.PositionImporter
{
  public class GpxPositionsImporter : PositionsImporter
  {
    public override bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == "gpx";
    }

    protected override IEnumerable<Position> LoadPositions(FilePath trackFile)
    {
      List<Position> result = new List<Position>();

      using(var stream = new FileStream(trackFile, FileMode.Open))
      using (GpxReader reader = new GpxReader(stream))
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


    public GpxPositionsImporter(
      IPositionsRepository positionsRepository,
      IDirectionsProvider directions,
      ILoggerFactory loggerFactory,
      DateTime startDate) : base(
        positionsRepository,
        directions,
        loggerFactory,
        startDate)
    {
    }
  }
}