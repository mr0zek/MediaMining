using System;
using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using Xunit;

namespace MediaPreprocessor.Tests.Positions
{
  public class StopDetectionTests
  {
    private readonly IStopDetector _sut;

    public StopDetectionTests()
    {
      _sut = new StopDetector();
    }

    [Theory]
    [InlineData("2021-08-24 05:00", "2021-08-24 10:40", 1)]
    public void Check_count_of_stops(DateTime from, DateTime to, int count)
    {
      List<Position> positions = new List<Position>();
      for (DateTime date = from; date < @to; date = date.AddDays(1))
      {
        string filePath = @$"data\{date.Date:yyyy-MM-dd}.geojson";
        positions.AddRange(Track.Load(filePath).Positions);
      }

      var result = _sut.Detect(positions.Where(f => f.Date >= from && f.Date <= to));

      Assert.Equal(count, result.Count());
    }
  }
}
