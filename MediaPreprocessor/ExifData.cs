using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MediaPreprocessor
{
  internal class ExifData
  {
    internal class Coordinate
    {
      public Coordinate(double latitude, double longitude)
      {
        Lat = latitude;
        Lon = longitude;
      }

      public double Lat { get; set; }
      public double Lon { get; set; }
    }

    public string Country { get; set; }
    public DateTime CreatedDate { get; set; }
    public string LocationName { get; set; }
    public Coordinate GPSLocation { get; set; }

    public List<string> ImageDescription { get; } = new List<string>();

    public ExifData(IDictionary<string, string> data)
    {
      // GPSLocation
      var lat = ConvertCoordinate(data["GPS Latitude"]);
      var lon = ConvertCoordinate(data["GPS Longitude"]);
      GPSLocation = new Coordinate(lat, lon);

      // CreatedDate
      var dateTagNames = new string[] { "Date/Time Original", "Create Date" };
      string key = data.Keys.First(f => dateTagNames.Any(x => x == f));
      string stringDate = data[key];
      stringDate = ReplaceFirst(ReplaceFirst(stringDate, ":", "-"), ":", "-");

      CreatedDate = DateTime.Parse(stringDate);
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
      stringBuilder.Append("-stay_open 0 -overwrite_original");
      if (GPSLocation != null)
      {
        stringBuilder.Append(
          $" -gpslatitude={GPSLocation.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        stringBuilder.Append(
          $" -gpslongitude={GPSLocation.Lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
      }

      if (LocationName != null)
      {
        stringBuilder.Append($" -City={LocationName}");
      }

      if (Country != null)
      {
        stringBuilder.Append($" -Country={Country}");
      }

      stringBuilder.Append($"-ImageDescription=\"{string.Join(";",ImageDescription)}\" -Description=\"{string.Join(";", ImageDescription)}\"");

      stringBuilder.Append($" \"{fileName}\"");

      using (Process myProcess = new Process())
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = "/exiftool/exiftool";
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.Arguments = stringBuilder.ToString();
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

        var o = myProcess.StandardOutput.ReadToEnd()
          .Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
          .Select(f =>
          {
            int index = f.IndexOf(':');
            string first = f.Substring(0, index);
            string second = f.Substring(index + 1);
            return new[] {first.Trim(), second.Trim()};
          }).GroupBy(f=>f[0]).Select(f=>f.First()).ToDictionary(f => f[0], f => f[1]);

        return new ExifData(o);
      }
    }

    public void UpdateGeolocationName()
    {
        HttpClient httpClient = new HttpClient();
        var s = "text/html,application/xhtml+xml,application/xml,image/avif,image/webp,image/apng,*/*,application/signed-exchange";
        foreach (var s1 in s.Split(","))
        {
          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(s1));
        }

        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "97.0.4692.71"));

        var task = httpClient.GetAsync($"https://nominatim.openstreetmap.org/reverse?format=json&lat={GPSLocation.Lat}&lon={GPSLocation.Lon}&zoom=18&addressdetails=1");
        task.Wait();
        var t2 = task.Result.Content.ReadAsStringAsync();
        t2.Wait();
        var geolocationData = JsonConvert.DeserializeObject<ReverseGeolocationResponse>(t2.Result);

        LocationName = geolocationData.GetLocationName();
        Country = geolocationData.Address.Country; 

        ImageDescription.Add(geolocationData.Display_Name);
    }
  }

  
}