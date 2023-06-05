using System.Collections.Generic;

namespace MediaPreprocessor.Positions.StopDetection
{
  public class StopsAndTracks
  {
    public List<Track> Tracks { get; internal set; } = new List<Track>();
    public List<Stop> Stops { get; internal set; } = new List<Stop>();
  }
}