using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using MediaPreprocessor.Directions;
using MediaPreprocessor.Events;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.Handlers;
using MediaPreprocessor.Handlers.MediaImportHandlers;
using MediaPreprocessor.Handlers.PostImportHandlers;
using MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator;
using MediaPreprocessor.Importers;
using MediaPreprocessor.MapGenerator;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor
{
  public class Bootstrap
  {
    //Media, Movie, Track, Action

    public void Run(string[] args, int count = 0, bool runInParallel = true)
    {
      var container = BuildContainer(args);

      var log = container.Resolve<ILoggerFactory>().CreateLogger(GetType());
      log.LogInformation("Application starting ...");

      IImporters importers = container.Resolve<IImporters>();

      if (count > 0)
      {
        for (int i = 0; i < count; i++)
        {
          importers.Import(runInParallel);
        }

        return;
      }

      while (true)
      {
        importers.Import(runInParallel);

        Thread.Sleep(5000);
      }
    }

    private IContainer BuildContainer(string[] args)
    {
      var builder = new ContainerBuilder();

      var options = args.Select(f =>
      {
        var sp = f.Split("=");
        return new KeyValuePair<string, string>(sp[0].ToLower(), sp[1]);
      }).ToDictionary(f=>f.Key, f => f.Value);

      if(options.ContainsKey("basepath"))
      {
        ExifData.ExifToolPath = $"{options["basepath"]}\\exiftool\\exiftool.exe";

        if (!options.ContainsKey("destination"))
        {
          options["destination"] = Path.Combine(options["basepath"], "destination");
        }
        if (!options.ContainsKey("source"))
        {
          options["source"] = Path.Combine(options["basepath"], "source");
        }
        if (!options.ContainsKey("events"))
        {
          options["events"] = Path.Combine(options["basepath"], "destination");
        }
        if (!options.ContainsKey("positions"))
        {
          options["positions"] = Path.Combine(options["basepath"], "positions");
        }
        if (!options.ContainsKey("geolocation"))
        {
          options["geolocation"] = Path.Combine(options["basepath"], "geolocation");
        }
        if (!options.ContainsKey("eventsLog"))
        {
          options["eventslog"] = Path.Combine(options["basepath"], "destination");
        }        
      }

      string destination = options.ContainsKey("destination") ? options["destination"] : "/data/destination";
      string source = options.ContainsKey("source") ? options["source"] : "/data/source";
      string events = options.ContainsKey("events") ? options["events"] : "/data/destination";
      string positions = options.ContainsKey("positions") ? options["positions"] : "/data/positions";
      string geolocation = options.ContainsKey("geolocation") ? options["geolocation"] : "/data/geolocation";
      string eventsLog = options.ContainsKey("eventslog") ? options["eventslog"] : "/data/eventsLog";


      // Register individual components
      builder.RegisterInstance(LoggerFactory.Create(b =>
      {
        b.AddConsole();
        b.SetMinimumLevel(LogLevel.Debug);
      })).AsImplementedInterfaces();
      builder.RegisterType<Importers.Importers>().AsImplementedInterfaces();
      builder.RegisterType<EventRepository>().WithParameter("eventsPath", events).SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<PositionsRepository>().WithParameter("basePath", positions).SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<MediaRepository>()
        .WithParameter("basePath", destination)
        .WithParameter("knownFilesExtensions", new[] { "mp4", "mts", "mov", "jpg", "jpeg", "webp" })
        .AsImplementedInterfaces();
      builder.RegisterType<Geolocation.Geolocation>().WithParameter("filePath", geolocation).SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<Inbox>().WithParameter("sourcePath", new []{ source}).AsImplementedInterfaces();
      builder.RegisterType<EventLogFactory>().AsImplementedInterfaces();      
      builder.RegisterType<StopDetector>().AsImplementedInterfaces();
      builder.RegisterType<DirectionsProvider>().AsImplementedInterfaces();
      builder.RegisterType<MapGenerator.MapGenerator>().AsImplementedInterfaces();
      builder.RegisterType<GeojsonGenerator>().WithParameter("basePath", destination).AsImplementedInterfaces();      
      builder.RegisterType<CalculateDailyStats>().AsImplementedInterfaces().WithParameter("outputFileName", "/data/positions/distanceStats.csv");
      builder.RegisterType<MediaTypeDetector>().AsImplementedInterfaces()
        .WithParameter("moviesExtensions", new[] { "mp4", "mts", "mov" })
        .WithParameter("photosExtensions", new[] { "jpg", "jpeg", "webp" });
      builder.RegisterType<EventLogGenerator>().AsImplementedInterfaces().WithParameter("basePath", eventsLog);
      builder.RegisterType<ActivityTypeDetector>().AsImplementedInterfaces();

      RegisterExternals(builder, options);

      return builder.Build();
    }

    protected virtual void RegisterExternals(ContainerBuilder builder, IDictionary<string,string> options)
    {
      
    }
  }
}