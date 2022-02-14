using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPreprocessor.Positions
{
  public class StopDetection : IStopDetection
  {
    public IEnumerable<Tuple<Position, TimeSpan>> Detect(IEnumerable<Position> positions)
    {
      List<Tuple<Position, double>> velocity = positions.Skip(1).SelectWithPrevious((prev, curr) =>
        new Tuple<Position, double>(curr, prev.DistanceTo(curr) / ((curr.Date - prev.Date).TotalSeconds / 60 / 60))).ToList();

      List<List<Position>> result = new List<List<Position>>();

      Position key = velocity.First().Item1;
      result.Add(new List<Position>() { key });
      foreach (var v in velocity)
      {
        if (v.Item2 < 5)
        {
          result.Last().Add(v.Item1);
        }
        else
        {
          if (result.Last().Count > 0)
          {
            result.Add(new List<Position>());
          }
        }
      } 
      result.Last().Add(positions.Last());

      var r2 = result.Select(f=> new Tuple<Position, TimeSpan>(
        f.First().CalculateCenter(f),
        f.Last().Date - f.First().Date));

      var r3 = r2.Where(f=> f == r2.First() || f == r2.Last() || f.Item2.TotalMinutes >= 10);

      return r3;
    }
  }
}