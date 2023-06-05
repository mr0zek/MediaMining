using MediaPreprocessor.Positions;
using System.Collections.Generic;

namespace MediaPreprocessor.Directions
{
  public class Directions
  {
    public Position From { get; private set; }
    public Position To { get; private set; }
    public IEnumerable<Position> Positions { get; set; }

    public Directions(IEnumerable<Position> positions, Position from, Position to)
    {
      Positions = positions;
      From = from;
      To = to;
    }
  }
}