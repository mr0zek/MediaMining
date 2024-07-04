using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using MediaMining.PositionImporter.GoogleTakeout;
using MediaPreprocessor.Directions;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaMining.PositionImporter
{
  public class GeojsonImporter : PositionsImporter
  {
    public override bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == "geojson";
    }

    protected override IEnumerable<Position> LoadPositions(FilePath trackFile)
    {
      var records = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(trackFile));
      return records.Features.Select(f => new Position(
          (f.Geometry as GeoJSON.Net.Geometry.Point).Coordinates.Latitude,
          (f.Geometry as GeoJSON.Net.Geometry.Point).Coordinates.Longitude,
          DateTime.Parse(f.Properties["reportTime"].ToString())));
    }

    public GeojsonImporter(
      IPositionsRepository positionsRepository, 
      IDirectionsProvider directionsPrivider,
      ILoggerFactory loggerFactory,
      DateTime startDate) : base(positionsRepository, directionsPrivider, loggerFactory, startDate)
    {
    }
  }
}