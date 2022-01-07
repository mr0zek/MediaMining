using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Geolocation;
using MediaPrep.GoogleJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaPrep
{
  class Program
  {
    private static ILogger logger;

    static void Main(string[] args)
    {
      var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("sourcePath", "/data/source") })
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("targetPath", "/data/destination") })
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("tracksPath", "/data/tracks") })
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("knownFileFormats", ".mp4|.jpg") })
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("runInterval", "5000") })
        .Build();

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder
          .AddConfiguration(configuration)
          .AddConsole(options =>
            {
              options.IncludeScopes = true;
              options.TimestampFormat = "hh:mm:ss ";
            });
      });

      logger = loggerFactory.CreateLogger<Program>();

      var knownFileFormats = configuration["knownFileFormats"].Split("|").Select(f=>f.ToLower());
      var sourcePath = configuration["sourcePath"];
      var targetPath = configuration["targetPath"];
      var tracksPath = configuration["tracksPath"];

      while (true)
      {
        logger.Log(LogLevel.Information, "Loading tracks information from : " + tracksPath);
        var coordinates = LoadGPSCoordinates(tracksPath);
        logger.Log(LogLevel.Information, $"Loaded {coordinates.Count} points"); 
        logger.Log(LogLevel.Information, "Scanning source directory : "+sourcePath);

        var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
        
        logger.Log(LogLevel.Information, $"Found {files.Length} files");
        
        foreach (var sourceFileName in files)
        {
          if (knownFileFormats.All(f => f != Path.GetExtension(sourceFileName).ToLower()))
          {
            continue;
          }

          logger.Log(LogLevel.Information, "Processing file: " + sourceFileName);
          var exifData = GetFileExifData(sourceFileName);

          var createdDate = GetCreateDateFromExif(exifData);
          var hasCoordinate = HasGPSCoordinate(exifData);

          logger.Log(LogLevel.Information, "- media create date : " + createdDate);
          logger.Log(LogLevel.Information, "- has GPS coordinates : " + hasCoordinate);

          string targetDirectory = Path.Combine(targetPath, createdDate.ToString("yyyy"), createdDate.ToString("yyyy-MM-dd"));

          Directory.CreateDirectory(targetDirectory);

          string targetFileName = Path.Combine(targetDirectory, Path.GetFileName(sourceFileName));

          if (File.Exists(targetFileName))
          {
            logger.Log(LogLevel.Information, "Cannot override file : "+targetFileName);
            continue;
          }

          File.Move(sourceFileName, targetFileName, true);

          if (!hasCoordinate)
          {
            Coordinate? coordinate = FindCoordinate(coordinates, createdDate);

            if (coordinate.HasValue)
            {
              SetGPSCoordinates(targetFileName, coordinate.Value);
            }
          }

          logger.Log(LogLevel.Information, "Moved to : " + targetFileName);
        }

        RemoveEmptyFoldersFromSource(sourcePath);

        Thread.Sleep(int.Parse(configuration["runInterval"]));
      }
    }

    private static bool RemoveEmptyFoldersFromSource(string sourcePath)
    {
      var directories = Directory.GetDirectories(sourcePath);
      foreach (var directory in directories)
      {
        if (RemoveEmptyFoldersFromSource(directory))
        {
          Directory.Delete(directory);
        }
      }

      var directories2 = Directory.GetDirectories(sourcePath);
      if (directories2.Length == 0)
      {
        if (Directory.GetFiles(sourcePath).Length == 0)
        {
          return true;
        }
      }

      return false;
    }

    private static Coordinate? FindCoordinate(IList<Tuple<DateTime, Coordinate>> coordinates, DateTime createdDate)
    {
      try
      {
        var coordinatesFromDay = coordinates.Where(f => f.Item1.Date == createdDate.Date);
        if (!coordinatesFromDay.Any())
        {
          logger.LogWarning("Cannot find coordinates for day : "+createdDate.ToShortDateString());
          return null;
        }

        var v = coordinatesFromDay.Min(x => Math.Abs((x.Item1 - createdDate).Ticks));

        DateTime closestDate = createdDate.AddTicks(-v);

        return coordinates.First(f => f.Item1 == closestDate).Item2;
      }
      catch(Exception ex)
      {
        logger.LogError(ex, "Error while searching for coordinate");
        return null;
      }
    }

    private static IList<Tuple<DateTime, Coordinate>> LoadGPSCoordinates(string tracksPath)
    {
      var result = new List<Tuple<DateTime, Coordinate>>();

      var trackFiles = Directory.GetFiles(tracksPath);

      foreach (var trackFile in trackFiles)
      {
        var records = JsonConvert.DeserializeObject<GoogleJsonRecords>(File.ReadAllText(trackFile));
        result.AddRange(records.Locations.Select(f =>
          new Tuple<DateTime, Coordinate>(f.Date, new Coordinate(f.Lat, f.Lng))));
      }

      return result;
    }

    private static void SetGPSCoordinates(string fileName, Coordinate coordinate)
    {
      using (Process myProcess = new Process())
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = "/exiftool/exiftool";
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.Arguments = $"-stay_open 0 -overwrite_original -gpslatitude={coordinate.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)} -gpslongitude={coordinate.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)} \"{fileName}\"";
        myProcess.StartInfo.CreateNoWindow = true;
        if (!myProcess.Start())
        {
          throw new Exception("Cannot find exiftool in path : " + myProcess.StartInfo.FileName);
        }

        myProcess.WaitForExit();
      }
    }

    private static bool HasGPSCoordinate(IEnumerable<string[]> exifData)
    {
      if(!exifData.Any(f => f.First() == "GPS Latitude"))
      {
        return false;
      }

      if(!exifData.Any(f => f.First() == "GPS Longitude"))
      {
        return false;
      }

      return true;
    }

    private static DateTime GetCreateDateFromExif(IEnumerable<string[]> exifData)
    {
      var dateTagNames = new string[]{ "Date/Time Original", "Create Date" };

      string stringDate = exifData.First(f => dateTagNames.Any(x=> x == f.First()))[1];
      stringDate = ReplaceFirst(ReplaceFirst(stringDate, ":", "-"), ":", "-");

      return DateTime.Parse(stringDate);
    }

    private static IEnumerable<string[]> GetFileExifData(string file)
    {
      using (Process myProcess = new Process())
      {
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = "/exiftool/exiftool";
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.Arguments = $"-stay_open 0 \"{file}\"";
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
            return new [] { first.Trim(), second.Trim() };
          });

        return o;
      }
    }

    static string ReplaceFirst(string text, string search, string replace)
    {
      int pos = text.IndexOf(search);
      if (pos < 0)
      {
        return text;
      }
      return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
  }

}
