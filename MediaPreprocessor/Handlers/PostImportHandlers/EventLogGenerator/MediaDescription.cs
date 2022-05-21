using System;
using MediaPreprocessor.Media;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class MediaDescription
  {
    public MediaType Type { get; }
    public string Path { get; }
    public CoordinatesDescription Coordinates { get; }
    public DateTime CreatedDate { get; }

    public MediaDescription(MediaType type, string path, DateTime createdDate, CoordinatesDescription coordinates)
    {
      Type = type;
      Path = path;
      CreatedDate = createdDate;
      Coordinates = coordinates;
    }
  }
}