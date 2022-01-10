using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Events;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaPreprocessor
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
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("eventsPath", "/data/events") })
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

      var knownFileFormats = configuration["knownFileFormats"].Split("|").Select(f=>f.ToLower()).ToArray();
      var sourcePath = configuration["sourcePath"];
      var targetPath = configuration["targetPath"];
      var tracksPath = configuration["tracksPath"];
      var eventsPath = configuration["eventsPath"];

      while (true)
      {
        EventsRoot events = EventsRoot.LoadFromPath(eventsPath);
        logger.Log(LogLevel.Information, "Loading tracks information from : " + tracksPath);
        Dictionary<string, List<Tuple<DateTime, IPosition>>> coordinates =
          new Dictionary<string, List<Tuple<DateTime, IPosition>>>();
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

          string targetFileName = RelocateFile(targetPath, createdDate, sourceFileName, events);
          if (targetFileName == null)
          {
            continue;
          }

          if (!hasCoordinate)
          {
            LoadGPSCoordinates(coordinates, createdDate, tracksPath);

            IPosition coordinate = FindCoordinate(coordinates, createdDate);

            if (coordinate != null)
            {
              SetGPSCoordinates(targetFileName, coordinate);
            }
          }

          logger.Log(LogLevel.Information, "Moved to : " + targetFileName);
        }

        RemoveEmptyFoldersFromSource(sourcePath);

        Thread.Sleep(int.Parse(configuration["runInterval"]));
      }
    }

    private static string RelocateFile(string targetPath, DateTime createdDate, string sourceFileName, EventsRoot events)
    {
      string eventName = events.GetEventName(createdDate);
      string targetDirectory = Path.Combine(targetPath, createdDate.ToString("yyyy"), createdDate.ToString("yyyy-MM-dd"));
      if (eventName != null)
      {
        targetDirectory = Path.Combine(targetPath, createdDate.ToString("yyyy"), eventName, createdDate.ToString("yyyy-MM-dd"));
      }
      string targetFileName = Path.Combine(targetDirectory, Path.GetFileName(sourceFileName));

      if (File.Exists(targetFileName))
      {
        logger.Log(LogLevel.Information, "Cannot override file : " + targetFileName);
        return null;
      }

      if (sourceFileName == targetFileName)
      {
        return targetFileName;
      }

      Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
      File.Move(sourceFileName, targetFileName, true);

      return targetFileName;
    }

    private static bool RemoveEmptyFoldersFromSource(string sourcePath)
    {
      try
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
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while removing empty folders");
      }

      return false;
    }

    private static IPosition FindCoordinate(Dictionary<string, List<Tuple<DateTime, IPosition>>> coordinates, DateTime createdDate)
    {
      try
      {
        var coordinatesFromDay = coordinates[createdDate.Year.ToString()].Where(f => f.Item1.Date == createdDate.Date).OrderBy(f=>f.Item1).ToList();
        if (!coordinatesFromDay.Any())
        {
          logger.LogWarning("Cannot find coordinates for day : "+createdDate.ToShortDateString());
          return null;
        }

        for (int i = 0; i < coordinatesFromDay.Count(); i++)
        {
          if (coordinatesFromDay[i].Item1 > createdDate)
          {
            if (i > 0)
            {
              if (Math.Abs((coordinatesFromDay[i].Item1 - createdDate).TotalSeconds) > Math.Abs((coordinatesFromDay[i - 1].Item1 - createdDate).TotalSeconds))
              {
                return coordinatesFromDay[i - 1].Item2;
              }

              return coordinatesFromDay[i].Item2;
            }

            return coordinatesFromDay[0].Item2;
          }
        }

        return coordinatesFromDay.Last().Item2;
      }
      catch(Exception ex)
      {
        logger.LogError(ex, "Error while searching for coordinate");
        return null;
      }
    }

    private static void LoadGPSCoordinates(Dictionary<string, List<Tuple<DateTime, IPosition>>> coordinates, DateTime date, string tracksPath)
    {
      if (coordinates.ContainsKey(date.Year.ToString()))
      {
        return;
      } 
      
      var trackFiles = Directory.GetFiles(Path.Combine(tracksPath, date.Year.ToString()));
      var result = coordinates[date.Year.ToString()] = new List<Tuple<DateTime, IPosition>>(); 
      
      foreach (var trackFile in trackFiles)
      {
        FeatureCollection features = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText(trackFile));

        var positions = features.Features.Select(f =>
          new Tuple<DateTime, IPosition>(DateTime.Parse(f.Properties["reportTime"].ToString()),
            (f.Geometry as Point).Coordinates));

        result.AddRange(positions);
      }
    }

    private static void SetGPSCoordinates(string fileName, IPosition coordinate)
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
