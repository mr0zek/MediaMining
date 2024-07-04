// See https://aka.ms/new-console-template for more information
using MediaPreprocessor.Events;
using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;
using System.Collections.Generic;

public interface ITimelineGenerator
{
  void Generate(Event ev, List<Media> media, DirectoryPath directory);
}
