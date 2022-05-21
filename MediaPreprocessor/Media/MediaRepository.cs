using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Events;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  class MediaRepository : IMediaRepository
  {
    private readonly DirectoryPath _basePath;
    private readonly IEventRepository _eventRepository;
    private readonly Dictionary<MediaId, Media> _index = new();
    private readonly IMediaTypeDetector _mediatypeDetector;

    public MediaRepository(string basePath, IEventRepository eventRepository, IMediaTypeDetector mediatypeDetector)
    {
      _basePath = basePath;
      _eventRepository = eventRepository;
      _mediatypeDetector = mediatypeDetector;
    }

    public Media Get(MediaId eventMediaId)
    {
      return _index[eventMediaId];
    }

    public void SaveOrUpdate(Media media)
    {
      ExifData exifData = ExifData.LoadFromFile(media.Path);
      exifData.GPSLocation = media.GpsLocation;
      exifData.CreatedDate = media.CreatedDate;
      exifData.LocationName = media.LocationName;
      exifData.Country = media.Country;
      exifData.WriteToFile(media.Path);

      var targetFileName = CalculateTargetPath(media);

      if (targetFileName != media.Path)
      {
        targetFileName.Directory.Create();
        File.Move(media.Path, targetFileName, true);
      }

      media.Path = targetFileName;

      _index[media.MediaId] = media;
    }

    public void AddToProcess(Media media)
    {
      var targetFileName = _basePath.AddDirectory("Processing").ToFilePath(media.Path.FileName);
      targetFileName.Directory.Create();
      
      File.Copy(media.Path, targetFileName, true);
      
      media.Path = targetFileName;

      _index[media.MediaId] = media;
    }

    public IEnumerable<Media> GetAll(Date dateFrom, Date dateTo)
    {
      List<string> files = new();

      for (Date date = dateFrom; date <= dateTo; date += 1)
      {
        var targetDirectory = _basePath.AddDirectory(dateFrom.ToString("yyyy"));
        Event @event = _eventRepository.GetByDate(date);
        if (@event != null)
        {
          targetDirectory = targetDirectory.AddDirectory(@event.GetUniqueName());
        }
        else
        {
          targetDirectory = targetDirectory.AddDirectory(date.ToString("yyyy-MM-dd"));
        }

        if (targetDirectory.Exists)
        {
          files.AddRange(
            Directory.GetFiles(targetDirectory, "*.*", SearchOption.AllDirectories).Except(files.ToArray()));
        }
      }

      var result = files.Select(f => Media.FromFile(f, MediaId.NewId(), _mediatypeDetector.Detect(f))).ToArray();

      foreach (var media in result)
      {
        _index[media.MediaId] = media;
      }

      return result;
    }

    private FilePath CalculateTargetPath(Media media)
    {
      var targetDirectory = _basePath.AddDirectory(media.CreatedDate.ToString("yyyy"),media.CreatedDate.ToString("yyyy-MM-dd"));

      if (media.EventId != null)
      {
        Event ex = _eventRepository.Get(media.EventId);
        targetDirectory = _basePath.AddDirectory(media.CreatedDate.ToString("yyyy"), ex.GetUniqueName(),
          media.CreatedDate.ToString("yyyy-MM-dd"));
      }

      if (media.GpsLocation == null)
      {
        targetDirectory = targetDirectory.AddDirectory("No-Gps-Location");
      }

      return targetDirectory.ToFilePath(media.Path.FileName);
    }
  }
}