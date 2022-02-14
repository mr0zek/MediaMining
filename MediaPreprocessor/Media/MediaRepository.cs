using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPreprocessor.Excursions;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  class MediaRepository : IMediaRepository
  {
    private readonly string _basePath;
    private readonly IExcursionRepository _excursionRepository;
    private readonly Dictionary<MediaId, Media> _index = new();

    public MediaRepository(string basePath, IExcursionRepository excursionRepository)
    {
      _basePath = basePath;
      _excursionRepository = excursionRepository;
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

    private string CalculateTargetPath(Media media)
    {
      string targetDirectory =
        Path.Combine(_basePath, media.CreatedDate.ToString("yyyy"), media.CreatedDate.ToString("yyyy-MM-dd"));

      if (media.ExcursionId != null)
      {
        Excursion ex = _excursionRepository.Get(media.ExcursionId);
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