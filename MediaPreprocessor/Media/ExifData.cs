using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using MediaPreprocessor.Positions;
using Newtonsoft.Json;

namespace MediaPreprocessor.Media
{
  public class ExifData
  {
    public string Country { get; set; }
    public DateTime CreatedDate { get; set; }
    public string LocationName { get; set; }
    public Position GPSLocation { get; set; }

    public ExifData(IDictionary<string, string> data)
    {
      // CreatedDate
      var dateTagNames = new [] { "Date/Time Original", "Create Date", "File Modification Date/Time" };
      
      string key = dateTagNames.First(f=> data.ContainsKey(f) && data[f] != "0000-00-00 00:00:00");
      string stringDate = data[key];
      stringDate = ReplaceFirst(ReplaceFirst(stringDate, ":", "-"), ":", "-");

      CreatedDate = DateTime.Parse(stringDate);

      // GPSLocation
      if (data.ContainsKey("GPS Latitude") && data.ContainsKey("GPS Longitude"))
      {
        var lat = ConvertCoordinate(data["GPS Latitude"]);
        var lon = ConvertCoordinate(data["GPS Longitude"]);
        GPSLocation = new Position(lat, lon, CreatedDate);
      }
    }

    private static string ReplaceFirst(string text, string search, string replace)
    {
      int pos = text.IndexOf(search);
      if (pos < 0)
      {
        return text;
      }
      return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    private double ConvertCoordinate(string coordinate)
    {
      var r = Regex.Match(coordinate, "(?<deg>[0-9.]+)\\s+deg\\s+(?<min>[0-9.]+)'\\s+(?<sec>[0-9.]+)\"\\s*([EWNS])?");
      double deg = double.Parse(r.Groups["deg"].Value);
      double min = double.Parse(r.Groups["min"].Value);
      double sec = double.Parse(r.Groups["sec"].Value);
      string s = r.Groups["EWNS"].Value;
      if (s.ToUpper() == "W" || s.ToUpper() == "S")
      {
        return - deg + min / 60 + sec / 3600;
      }

      return deg + min / 60 + sec / 3600;
    }

    public void WriteToFile(string fileName)
    {
      
      StringBuilder stringBuilder = new StringBuilder();
      if (GPSLocation != null)
      {
        stringBuilder.Append(
          $" -gpslatitude={GPSLocation.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        stringBuilder.Append(
          $" -gpslongitude={GPSLocation.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
      }

      stringBuilder.Append($" -DateTimeOriginal=\"{CreatedDate.ToString("yyyy-MM-dd hh:mm:ss")}\"");
      stringBuilder.Append($" -FileModifyDate=\"{CreatedDate.ToString("yyyy-MM-dd hh:mm:ss")}\"");
      
      if (LocationName != null)
      {
        stringBuilder.Append($" -LocationName=\"{LocationName.Replace("\"","'")}\"");
      }

      if (Country != null)
      {
        stringBuilder.Append($" -Country=\"{Country}\"");
      }

      using (Process myProcess = new Process())
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = "/exiftool/exiftool";
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.Arguments = $"-stay_open 0 -overwrite_original {stringBuilder} \"{fileName}\"";
        myProcess.StartInfo.CreateNoWindow = true;
        if (!myProcess.Start())
        {
          throw new Exception("Cannot find exiftool in path : " + myProcess.StartInfo.FileName);
        }

        myProcess.WaitForExit();
        if (myProcess.ExitCode != 0)
        {
          throw new Exception("Cannot save exif data : " + myProcess.StandardOutput.ReadToEnd());
        }
      }
    }

    public static ExifData LoadFromFile(string fileName)
    {
      using (Process myProcess = new Process())
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = "/exiftool/exiftool";
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.Arguments = $"-stay_open 0 \"{fileName}\"";
        myProcess.StartInfo.CreateNoWindow = true;
        if (!myProcess.Start())
        {
          throw new Exception("Cannot find exiftool in path : " + myProcess.StartInfo.FileName);
        }

        myProcess.WaitForExit();
        if (myProcess.ExitCode != 0)
        {
          throw new Exception("Cannot read exif data : " + myProcess.StandardOutput.ReadToEnd());
        }

        var o = myProcess.StandardOutput.ReadToEnd()
          .Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
          .Select(f =>
          {
            int index = f.IndexOf(':');
            string first = f.Substring(0, index);
            string second = f.Substring(index + 1);
            return new[] {first.Trim(), second.Trim()};
          }).GroupBy(f=>f[0]).Select(f=>f.First()).ToDictionary(f => f[0], f => f[1]);

        if (!o.Any())
        {
          throw new InvalidOperationException();
        }

        return new ExifData(o);
      }
    } 
  }

  
}