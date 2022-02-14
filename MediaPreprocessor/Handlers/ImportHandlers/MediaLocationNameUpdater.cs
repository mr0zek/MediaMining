using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Media;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  public class MediaLocationNameUpdater : IMediaImportHandler
  {
    private readonly IMediaRepository _mediaRepository;
    private readonly IGeolocation _geolocation;

    public MediaLocationNameUpdater(IMediaRepository mediaRepository, IGeolocation geolocation)
    {
      _mediaRepository = mediaRepository;
      _geolocation = geolocation;
    }

    public void Handle(Media.Media media)
    {
      if (media.GpsLocation != null && media.LocationName == null)
      {
        var location = _geolocation.GetReverseGeolocationData(media.GpsLocation);
        media.LocationName = location.GetLocationName();
        media.Country = location.GetCountry();
      }
    }
  }
}