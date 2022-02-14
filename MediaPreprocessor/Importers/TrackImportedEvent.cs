using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Importers
{
  internal class TrackImportedEvent : IEvent
  {
    public MediaId MediaId { get; }

    public TrackImportedEvent(MediaId mediaId)
    {
      MediaId = mediaId;
    }
  }
}