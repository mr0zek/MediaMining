using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPreprocessor.Positions;
using Xunit;

namespace MediaPreprocessor.Tests.Positions
{
  public class TrackTests
  {
    [Fact]
    public void Compact_should_remove_start_and_end_points()
    {
      Track track = new Track(new []
      {
        new Position(45,23, new DateTime(2022,01,01,7,23,0)),
        new Position(45.01,23, new DateTime(2022,01,01,8,23,0)),
        new Position(45.02,23, new DateTime(2022,01,01,10,23,0)),
        new Position(45.03,23, new DateTime(2022,01,01,10,24,0)),
        new Position(45.04,23, new DateTime(2022,01,01,10,25,0)),
        new Position(45.05,23, new DateTime(2022,01,01,10,26,0)),
        new Position(45.06,23, new DateTime(2022,01,01,11,26,0)),
        new Position(45.07,23, new DateTime(2022,01,01,12,26,0))
      });

      var t2 = track.Compact();
      Assert.Equal(6, t2.Positions.Count());
    }
  }
}
