using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Importers
{
  internal class ExcursionImportedEvent : IEvent
  {
    public MediaId MediaId { get; }

    public ExcursionImportedEvent(MediaId mediaId)
    {
      MediaId = mediaId;
    }
  }
}