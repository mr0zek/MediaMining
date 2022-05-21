using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public interface IActivityTypeDetector
  {
    ActivityType Detect(Track track);
  }

  class ActivityTypeDetector : IActivityTypeDetector
  {
    public ActivityType Detect(Track track)
    {
      //TODO: implement
      return ActivityType.Unknown;
    }
  }
}