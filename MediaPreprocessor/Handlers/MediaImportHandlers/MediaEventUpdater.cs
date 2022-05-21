using MediaPreprocessor.Events;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Handlers.MediaImportHandlers
{
  public class MediaEventUpdater : IMediaImportHandler
  {
    private readonly IEventRepository _eventRepository;
    private readonly ILogger _log;

    public MediaEventUpdater(IEventRepository eventRepository, ILoggerFactory loggerFactory)
    {
      _eventRepository = eventRepository;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Handle(Media.Media media)
    {
      if (media.EventId == null)
      {
        var e = _eventRepository.GetByDate(media.CreatedDate);
        if (e != null)
        {
          media.EventId = e.Id;
          _log.LogInformation($"Event information updated in file: {media.Path} - {e.GetUniqueName()}");
        }
      }
    }
  }
}