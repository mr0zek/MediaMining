using GeoJSON.Net.Feature;
using MediaPreprocessor.Events;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MediaPreprocessor.MapGenerator
{
  internal class MapGenerator : IMapGenerator
  {
    private IGeojsonGenerator _geojsonGenerator;

    public MapGenerator(IGeojsonGenerator geojsonGenerator)
    {
      _geojsonGenerator = geojsonGenerator;
    }

    public string GenerateMapFile(MapGeneratorOptions mapGeneratorOptions)
    {
      var path = DirectoryPath.Parse(System.Environment.CurrentDirectory)
        .AddDirectory("MapGenerator")
        .ToFilePath("mapTemplate.html");

      string mapTemplate = File.ReadAllText(path);
      foreach (var prop in mapGeneratorOptions.GetType().GetProperties())
      {
        mapTemplate = mapTemplate.Replace($"@{prop.Name}@", prop.GetValue(mapGeneratorOptions, null).ToString());
      }

      return mapTemplate;
    }

    public void Generate(Event ev, IEnumerable<Media.Media> medias, DirectoryPath outputDirectory)
    {
      outputDirectory.Create();

      FeatureCollection fc = _geojsonGenerator.Generate(ev, medias);

      var geojsonFilePath = outputDirectory.ToFilePath(ev.DateFrom.ToString("yyyy-MM-dd") + " - " + ev.Name + ".geojson");

      File.WriteAllText(geojsonFilePath, JsonConvert.SerializeObject(fc, Formatting.Indented));

      FilePath mapFile = outputDirectory.ToFilePath("index.html");
      mapFile.Directory.Create();

      string map = GenerateMapFile(new MapGeneratorOptions { FilePath = geojsonFilePath.FileName, Title = ev.GetUniqueName() });

      File.WriteAllText(mapFile, map);

      FilePath batFile = outputDirectory.ToFilePath("run.bat");
      File.WriteAllText(batFile,
        @"start http-server
          start http://localhost:8080");      
    }
  }
}
