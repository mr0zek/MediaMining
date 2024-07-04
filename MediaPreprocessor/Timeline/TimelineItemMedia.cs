// See https://aka.ms/new-console-template for more information
using MediaPreprocessor.Media;

public class TimelineItemMedia
{
  public string Name { get; internal set; }
  public MediaType Type { get; internal set; }

  public MediaSource Source { get; internal set; }
}