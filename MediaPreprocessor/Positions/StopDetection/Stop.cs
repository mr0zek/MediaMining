using System;

namespace MediaPreprocessor.Positions.StopDetection
{
  public class Stop
  {
    public Position Position { get; }
    public DateTime DateTo { get; }
    public DateTime DateFrom { get; set; }

    public Stop(Position position, DateTime dateFrom, DateTime dateTo)
    {
      Position = position;
      DateFrom = dateFrom;
      DateTo = dateTo;
    }

    public TimeSpan Duration()
    {
      return DateTo - DateFrom;
    }
  }
}