using System;
using System.IO;
using MediaPreprocessor.Events;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  public class Media
  {
    private Position _gpsLocation;
    private bool _dirty;
    private DateTime _createdDate;
    private string _locationName;
    private string _country;

    private Media(MediaId mediaId, MediaType type, Position gpsLocation, DateTime createdDate, string path)
    {
      MediaId = mediaId;
      Type = type;
      GpsLocation = gpsLocation;
      CreatedDate = createdDate;
      Path = path;
      _dirty = false;
    }

    public Position GpsLocation
    {
      get => _gpsLocation;
      set
      {
        _gpsLocation = value;
        _dirty = true;
      }
    }

    public MediaId MediaId { get; }
    public EventId EventId { get; set; }

    public DateTime CreatedDate
    {
      get => _createdDate;
      set
      {
        _createdDate = value;
        _dirty = true;
      }
    }

    public FilePath Path { get; private set; }

    public string LocationName
    {
      get => _locationName;
      set
      {
        _locationName = value;
        _dirty = true;
      }
    }

    public string Country
    {
      get => _country;
      set
      {
        _country = value;
        _dirty = true;
      }
    }

    public MediaType Type { get; set; }

    public static Media FromFile(FilePath filePath)
    {
      return FromFile(filePath, MediaId.NewId(), new MediaTypeDetector().Detect(filePath));
    }

    public static Media FromFile(FilePath filePath, MediaId eventMediaId, MediaType mediaType)
    {
      if (!File.Exists(filePath))
      {
        throw new ArgumentException("File not exists : " + filePath);
      }
      var exifData = ExifData.LoadFromFile(filePath);
      return new Media(eventMediaId, mediaType, exifData.GPSLocation, exifData.CreatedDate, filePath);
    }

    public void SaveAll()
    {
      if (_dirty)
      {
        ExifData exifData = ExifData.LoadFromFile(Path);
        exifData.GPSLocation = GpsLocation;
        exifData.CreatedDate = CreatedDate;
        exifData.LocationName = LocationName;
        exifData.Country = Country;
        exifData.WriteToFile(Path);
        _dirty = false;
      }
    }
    
    public void MoveTo(FilePath filePath)
    {
      if (Path == filePath)
      {
        return;
      }
      filePath.Directory.Create();
      File.Move(Path, filePath);
      Path = filePath;
    }
  }
}