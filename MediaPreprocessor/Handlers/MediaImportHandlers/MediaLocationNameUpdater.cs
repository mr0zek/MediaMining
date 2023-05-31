using MediaPreprocessor.Geolocation;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Handlers.MediaImportHandlers
{
  public class MediaLocationNameUpdater 
  {
    private readonly IGeolocation _geolocation;
    private readonly ILogger _log;

    public MediaLocationNameUpdater(IGeolocation geolocation, ILoggerFactory loggerFactory)
    {
      _geolocation = geolocation;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Handle(Media.Media media)
    {
      if (media.GpsLocation != null && media.LocationName == null)
      {
        var location = _geolocation.GetReverseGeolocationData(media.GpsLocation);
        media.LocationName = location.LocationName;
        media.Country = location.Country;
        _log.LogInformation($"Location information updated in file: {media.Path} - {media.LocationName}");
      }
    }
  }
}