using MediaPreprocessor.Events;
using MediaPreprocessor.Importers;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  public class MediaEventUpdater : IMediaImportHandler
  {
    private readonly IEventRepository _eventRepository;

    public MediaEventUpdater(IEventRepository eventRepository)
    {
      _eventRepository = eventRepository;
    }

    public void Handle(Media.Media media)
    {
      if (media.EventId == null)
      {
        media.EventId = _eventRepository.GetByDate(media.CreatedDate)?.Id;
      }
    }
  }
}