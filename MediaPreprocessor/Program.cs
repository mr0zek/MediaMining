using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        .AddInMemoryCollection(new KeyValuePair<string, string>[] { new("knownFileFormats", ".mp4|.jpg|.mts") })
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

        GPSCoordinates gpsCoordinates = new GPSCoordinates(tracksPath);

        logger.Log(LogLevel.Information, $"Loaded {gpsCoordinates.Count} points"); 
        logger.Log(LogLevel.Information, "Scanning source directory : "+sourcePath);

        var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
        
        logger.Log(LogLevel.Information, $"Found {files.Length} files");
        
        foreach (var sourceFileName in files)
        {
          if (knownFileFormats.All(f => f != Path.GetExtension(sourceFileName).ToLower()))
          {
            continue;
          }

          try
          {
            logger.Log(LogLevel.Information, "Processing file: " + sourceFileName);
            ExifData exifData = ExifData.LoadFromFile(sourceFileName);

            logger.Log(LogLevel.Information, "- media create date : " + exifData.CreatedDate);            

            if (exifData.GPSLocation == null)
            {
              gpsCoordinates.LoadCoordinatesForDate(exifData.CreatedDate);

              var position = gpsCoordinates.Find(exifData.CreatedDate);

              if (position == null)
              {
                logger.LogWarning("Cannot find coordinates for day : " + exifData.CreatedDate.ToShortDateString());
              }
              else
              {
                exifData.GPSLocation = new ExifData.Coordinate(position.Latitude, position.Longitude);
              }
            }
            
            string targetFileName = RelocateFile(targetPath, exifData, sourceFileName, events);
            if (targetFileName == null)
            {
              continue;
            }

            logger.Log(LogLevel.Information, $"- GPS coordinate : {exifData.GPSLocation.Lat},{exifData.GPSLocation.Lon}");

            exifData.UpdateGeolocationName();

            exifData.WriteToFile(targetFileName);

            logger.Log(LogLevel.Information, "Moved to : " + targetFileName);
          }
          catch (Exception ex)
          {
            logger.LogError(ex,"Error while processing file : "+sourceFileName);
          }
        }

        RemoveEmptyFoldersFromSource(sourcePath);

        Thread.Sleep(int.Parse(configuration["runInterval"]));
      }
    }

    private static string RelocateFile(string targetPath, ExifData exifData, string sourceFileName, EventsRoot events)
    {
      string eventName = events.GetEvent(exifData.CreatedDate)?.GetUniqueName();
      string targetDirectory = Path.Combine(targetPath, exifData.CreatedDate.ToString("yyyy"), exifData.CreatedDate.ToString("yyyy-MM-dd"));
      if (eventName != null)
      {
        targetDirectory = Path.Combine(targetPath, exifData.CreatedDate.ToString("yyyy"), eventName, exifData.CreatedDate.ToString("yyyy-MM-dd"));
      }
      string targetFileName = Path.Combine(targetDirectory, Path.GetFileName(sourceFileName));
      if(exifData.GPSLocation == null)
      {
        targetFileName = Path.Combine(Path.GetDirectoryName(sourceFileName), "No-Gps-Location", Path.GetFileName(sourceFileName));
      }

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
  }
}
