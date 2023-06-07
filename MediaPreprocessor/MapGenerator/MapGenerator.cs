using MediaPreprocessor.Shared;
using System.IO;

namespace MediaPreprocessor.MapGenerator
{
  internal class MapGenerator : IMapGenerator
  {
    public string Generate(MapGeneratorOptions mapGeneratorOptions)
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
  }
}
