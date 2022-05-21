using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Handlers.MediaImportHandlers
{
  public class MediaGpsLocationUpdater :  IMediaImportHandler
  {
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger _log;

    public MediaGpsLocationUpdater(IPositionsRepository positionsRepository, ILoggerFactory loggerFactory)
    {
      _positionsRepository = positionsRepository;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Handle(Media.Media media)
    {
      //if(media.GpsLocation == null)
      {
        media.GpsLocation = _positionsRepository.Get(media.CreatedDate);
        _log.LogInformation($"GPS information updated in file: {media.Path} - lat:{media.GpsLocation.Latitude}, lon:{media.GpsLocation.Longitude}");
      }
    }
  }
}