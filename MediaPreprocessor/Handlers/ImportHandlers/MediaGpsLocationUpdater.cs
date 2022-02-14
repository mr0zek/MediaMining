using MediaPreprocessor.Importers;
using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  public class MediaGpsLocationUpdater :  IMediaImportHandler
  {
    private readonly IPositionsRepository _positionsRepository;

    public MediaGpsLocationUpdater(IPositionsRepository positionsRepository)
    {
      _positionsRepository = positionsRepository;
    }

    public void Handle(Media.Media media)
    {
      if(media.GpsLocation == null)
      {
        media.GpsLocation = _positionsRepository.Get(media.CreatedDate);
      }
    }
  }
}