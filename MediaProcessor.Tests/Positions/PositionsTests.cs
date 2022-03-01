using System;
using MediaPreprocessor.Positions;
using Xunit;

namespace MediaProcessor.Tests
{
  public class PositionsTests
  {
    [Fact]
    public void Check_CalculateCenter()
    {
      var positions = new[]
      {
        new Position(45, 15, DateTime.Parse("2020-11-11 12:00")),
        new Position(46, 16, DateTime.Parse("2020-11-11 12:30")),
        new Position(47, 17, DateTime.Parse("2020-11-11 14:00"))
      };

      var p = Position.CalculateCenter(positions);

      Assert.Equal(46, p.Latitude);
      Assert.Equal(16, p.Longitude);
      Assert.Equal(DateTime.Parse("2020-11-11 13:00"), p.Date);
    }
  }
}