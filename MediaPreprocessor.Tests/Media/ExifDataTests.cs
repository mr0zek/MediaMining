using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPreprocessor.Media;
using Xunit;

namespace MediaPreprocessor.Tests.Media
{
  public class ExifDataTests
  {
    [Fact]
    public void ReadWriteTest()
    {
      ExifData.ExifToolPath = AppDomain.CurrentDomain.BaseDirectory + "\\exifTool\\exifTool.exe";
      var data = ExifData.LoadFromFile(@"data\media\IMG_20200819_141455.jpg");

      Assert.Equal(DateTime.Parse("2020-08-19 14:14:56"), data.CreatedDate);
      Assert.Equal(50.397438888888885, data.GPSLocation.Latitude);
      Assert.Equal(23.18193888888889, data.GPSLocation.Longitude);

      string tmp = Path.GetTempFileName();
      double tmpLat = 52.34;
      double tmpLon = 24.11;
      File.Copy(@"data\media\IMG_20200819_141455.jpg", tmp, true);
      data.GPSLocation.Latitude = tmpLat;
      data.GPSLocation.Longitude = tmpLon;
      data.WriteToFile(tmp);

      var data2 = ExifData.LoadFromFile(tmp);

      Assert.Equal(DateTime.Parse("2020-08-19 14:14:56"), data2.CreatedDate);
      Assert.Equal(tmpLat, Math.Round(data2.GPSLocation.Latitude,2));
      Assert.Equal(tmpLon, Math.Round(data2.GPSLocation.Longitude,2));
    }
  }
}
