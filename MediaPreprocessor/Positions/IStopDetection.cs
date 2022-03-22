using System;
using System.Collections.Generic;
using MediaPreprocessor.Events.Log;

namespace MediaPreprocessor.Positions
{
  public interface IStopDetection
  {
    IEnumerable<Tuple<Position, TimeSpan>> Detect(IEnumerable<Position> positions);
  }
}