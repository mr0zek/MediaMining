using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPreprocessor.Positions
{
  public class StopDetection : IStopDetection
  {
    public IEnumerable<Tuple<Position, TimeSpan>> Detect(IEnumerable<Position> positions)
    {
      try
      {
        List<Tuple<Position, double>> velocity = positions.Skip(1).SelectWithPrevious((prev, curr) =>
            new Tuple<Position, double>(curr, prev.DistanceTo(curr) / ((curr.Date - prev.Date).TotalSeconds / 60 / 60)))
          .ToList();

        //  filter by velocity, distance and time
        List<List<Position>> result = new List<List<Position>>();
        result.Add(new List<Position>());
        foreach (var v in velocity)
        {
          if (v.Item2 < 2)
          {
            if(result.Count == 0 || 
               result.Last().Count == 0 || 
               (Position.CalculateCenter(result.Last()).DistanceTo(v.Item1) < 0.15 && (result.Last().Last().Date - v.Item1.Date).TotalMinutes < 30))
            {
              result.Last().Add(v.Item1);
            }
            else
            {
              result.Add(new List<Position>(){ v.Item1 });
            }
          }
        }

        // calculate central points and duration
        var r2 = result.Select(f => new Tuple<Position, TimeSpan>(
          Position.CalculateCenter(f),
          f.Last().Date - f.First().Date))
          .ToList();

        // filter by time
        var r4 = r2.Where(f=> f.Item2.TotalMinutes >= 10);

        return r4;
      }
      catch (Exception)
      {
        return new Tuple<Position, TimeSpan>[] { };
      }
    }
  }
}