using System;
using System.IO;
using MediaPreprocessor.Excursions;
using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Media
{
  public class Media
  {
    private Media(MediaId mediaId, Position gpsLocation, DateTime createdDate, string path)
    {
      MediaId = mediaId;
      GpsLocation = gpsLocation;
      CreatedDate = createdDate;
      Path = path;
    }

    public Position GpsLocation { get; set; }
    public MediaId MediaId { get; }
    public ExcursionId ExcursionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Path { get; set; }
    public string LocationName { get; set; }
    public string Country { get; set; }

    public static Media FromFile(string filePath, MediaId eventMediaId)
    {
      if (!File.Exists(filePath))
      {
        throw new ArgumentException("File not exists : " + filePath);
      }
      var exifData = ExifData.LoadFromFile(filePath);
      return new Media(eventMediaId, exifData.GPSLocation, exifData.CreatedDate, filePath);
    }
  }
}