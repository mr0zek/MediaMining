using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPreprocessor.Positions.StopDetection
{
  public class StopDetector : IStopDetector
  {
    public IEnumerable<Stop> Detect(IEnumerable<Position> positions)
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
        var r2 = result.Select(f => new Stop(
          Position.CalculateCenter(f),
          f.First().Date,
          f.Last().Date))
          .ToList();

        // filter by time
        var r4 = r2.Where(f=> f.Duration().TotalMinutes >= 10);

        return r4;
      }
      catch (Exception)
      {
        return Array.Empty<Stop>();
      }
    }
  }
}