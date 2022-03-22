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
    private readonly string _basePath;
    private readonly IEventRepository _eventRepository;
    private readonly Dictionary<MediaId, Media> _index = new();

    public MediaRepository(string basePath, IEventRepository eventRepository)
    {
      _basePath = basePath;
      _eventRepository = eventRepository;
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
        Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
        File.Move(media.Path, targetFileName, true);
      }

      media.Path = targetFileName;

      _index[media.MediaId] = media;
    }

    public void AddToProcess(Media media)
    {
      var targetFileName = Path.Combine(_basePath, "Processing", Path.GetFileName(media.Path));

      Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
      File.Copy(media.Path, targetFileName, true);
      
      media.Path = targetFileName;

      _index[media.MediaId] = media;
    }

    public IEnumerable<Media> GetAll(Date dateFrom, Date dateTo)
    {
      List<string> files = new();

      for (Date date = dateFrom; date < dateTo; date += 1)
      {
        var targetDirectory = Path.Combine(_basePath, dateFrom.ToString("yyyy"));
        Event @event = _eventRepository.GetByDate(date);
        if (@event != null)
        {
          targetDirectory = Path.Combine(targetDirectory, @event.GetUniqueName());
        }
        else
        {
          targetDirectory = Path.Combine(targetDirectory, date.ToString("yyyy-MM-dd"));
        }

        if (Directory.Exists(targetDirectory))
        {
          files.AddRange(
            Directory.GetFiles(targetDirectory, "*.*", SearchOption.AllDirectories).Except(files.ToArray()));
        }
      }

      var result = files.Select(f => Media.FromFile(f, MediaId.NewId())).ToArray();

      foreach (var media in result)
      {
        _index[media.MediaId] = media;
      }

      return result;
    }

    private string CalculateTargetPath(Media media)
    {
      string targetDirectory =
        Path.Combine(_basePath, media.CreatedDate.ToString("yyyy"), media.CreatedDate.ToString("yyyy-MM-dd"));

      if (media.EventId != null)
      {
        Event ex = _eventRepository.Get(media.EventId);
        targetDirectory = Path.Combine(_basePath, media.CreatedDate.ToString("yyyy"), ex.GetUniqueName(),
          media.CreatedDate.ToString("yyyy-MM-dd"));
      }

      if (media.GpsLocation == null)
      {
        targetDirectory = Path.Combine(targetDirectory, "No-Gps-Location");
      }

      string targetFileName = Path.Combine(targetDirectory, Path.GetFileName(media.Path));
      return targetFileName;
    }
  }
}