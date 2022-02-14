using MediaPreprocessor.Excursions;
using MediaPreprocessor.Importers;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  public class MediaExcursionUpdater : IMediaImportHandler
  {
    private readonly IExcursionRepository _excursionRepository;

    public MediaExcursionUpdater(IExcursionRepository excursionRepository)
    {
      _excursionRepository = excursionRepository;
    }

    public void Handle(Media.Media media)
    {
      if (media.ExcursionId == null)
      {
        media.ExcursionId = _excursionRepository.GetByDate(media.CreatedDate)?.Id;
      }
    }
  }
}