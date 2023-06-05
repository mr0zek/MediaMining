using MediaPreprocessor.Shared;
using System.IO;

namespace MediaPreprocessor.MapGenerator
{
  internal class MapGenerator : IMapGenerator
  {
    public string Generate(string geojson)
    {
      var path = DirectoryPath.Parse(System.Environment.CurrentDirectory)
        .AddDirectory("MapGenerator")
        .ToFilePath("mapTemplate.html");

      string mapTemplate = File.ReadAllText(path);
      return mapTemplate.Replace("@geojson@", geojson);
    }
  }
}
