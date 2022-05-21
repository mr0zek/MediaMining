using System;
using System.IO;
using MediaPreprocessor.Events;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  public class Media
  {
    private Media(MediaId mediaId, MediaType type, Position gpsLocation, DateTime createdDate, string path)
    {
      MediaId = mediaId;
      Type = type;
      GpsLocation = gpsLocation;
      CreatedDate = createdDate;
      Path = path;
    }

    public Position GpsLocation { get; set; }
    public MediaId MediaId { get; }
    public EventId EventId { get; set; }
    public DateTime CreatedDate { get; set; }
    public FilePath Path { get; set; }
    public string LocationName { get; set; }
    public string Country { get; set; }
    public MediaType Type { get; set; }

    public static Media FromFile(string filePath, MediaId eventMediaId, MediaType mediaType)
    {
      if (!File.Exists(filePath))
      {
        throw new ArgumentException("File not exists : " + filePath);
      }
      var exifData = ExifData.LoadFromFile(filePath);
      return new Media(eventMediaId, mediaType, exifData.GPSLocation, exifData.CreatedDate, filePath);
    }
  }
}