// See https://aka.ms/new-console-template for more information
using MediaPreprocessor.Media;
using System.Collections.Generic;

internal class TimelineItem
{
  public string CardTitle { get; internal set; }
  public string CardSubtitle { get; internal set; }
  public TimelineItemMedia Media { get; internal set; }
}