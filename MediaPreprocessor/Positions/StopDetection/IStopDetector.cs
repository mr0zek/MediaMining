using System.Collections.Generic;

namespace MediaPreprocessor.Positions.StopDetection
{
  public interface IStopDetector
  {
    IEnumerable<Stop> Detect(IEnumerable<Position> positions);
  }
}