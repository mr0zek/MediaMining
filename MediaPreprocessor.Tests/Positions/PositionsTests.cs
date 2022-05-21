using System;
using System.Globalization;
using MediaPreprocessor.Positions;
using Newtonsoft.Json;
using Xunit;

namespace MediaPreprocessor.Tests.Positions
{
  public class PositionsTests 
  {
    [Fact]
    public void CalculatePositionAtDateTest()
    {
      Position p1 = new Position(45, 10, DateTime.Parse("2022-01-01 00:00:00"));
      Position p2 = new Position(46, 11, DateTime.Parse("2022-01-01 00:10:00"));
      Position result = new Position(45.2, 10.2, DateTime.Parse("2022-01-01 00:02:00"));


      var p3 = Position.CalculatePositionAtDate(p1, p2, DateTime.Parse("2022-01-01 00:02:00"));
      Assert.Equal(p3,result);

      var p4 = Position.CalculatePositionAtDate(p1, p2, DateTime.Parse("2022-01-01 00:00:00"));
      Assert.Equal(p4, p1);

      var p5 = Position.CalculatePositionAtDate(p1, p2, DateTime.Parse("2022-01-01 00:10:00"));
      Assert.Equal(p5, p2);
    }

    [Fact]
    public void SerializeWithTimeZone()
    {
      DateTime t = DateTime.Now;
      var s = t.ToString("o");

      DateTime t2 = DateTime.Parse(s);

      Assert.Equal(t,t2);
    }

    class T2
    {
      public DateTime Date { get; set; }
    }

    [Fact]
    public void DeserializeDate()
    {
      T2 t = new T2();
      t.Date = DateTime.Parse("2021-08-25T07:11:04.011Z").ToUniversalTime();
      var d = DateTime.Parse("2021-08-24T03:40:37Z").ToLocalTime();

      var dstr = t.Date.ToString("o", CultureInfo.InvariantCulture);

      var s = JsonConvert.SerializeObject(t);

      var t2 = JsonConvert.DeserializeObject<T2>(s);

      Assert.Equal(t.Date,t2.Date);
    }

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