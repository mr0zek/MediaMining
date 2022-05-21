using System;
using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class TrackDescription
  {
    public ActivityType ActivityType { get; set; } = ActivityType.Unknown;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public double Distance { get; }
    public IList<MediaDescription> Media { get; set; } = new List<MediaDescription>();

    public TrackDescription(ActivityType activityType, Date dateFrom, Date dateTo, double distance)
    {
      ActivityType = activityType;
      DateFrom = dateFrom;
      DateTo = dateTo;
      Distance = distance;
    }
  }
}