using System;
using System.Globalization;
using System.Threading;
using Autofac;
using MediaPreprocessor.Events;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.Handlers;
using MediaPreprocessor.Handlers.MediaImportHandlers;
using MediaPreprocessor.Handlers.PositionImportHandlers;
using MediaPreprocessor.Handlers.PostImportHandlers;
using MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor
{
  public class Bootstrap
  {
    //Media, Movie, Track, Action

    public void Run()
    {
      var container = BuildContainer();

      var log = container.Resolve<ILoggerFactory>().CreateLogger(GetType());
      log.LogInformation("Application starting ...");

      IImporters importers = container.Resolve<IImporters>();

      while (true)
      {
        importers.Import();

        Thread.Sleep(5000);
      }
    }

    private IContainer BuildContainer()
    {
      var builder = new ContainerBuilder();

      // Register individual components
      builder.RegisterInstance(LoggerFactory.Create(b =>
      {
        b.AddConsole();
      })).AsImplementedInterfaces();
      builder.RegisterType<Importers.Importers>().WithParameter("deleteAfterImport", true).AsImplementedInterfaces();
      builder.RegisterType<MediaRepository>().WithParameter("basePath","/data/destination").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<EventRepository>().WithParameter("eventsPath", "/data/events").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<PositionsRepository>().WithParameter("basePath", "/data/positions").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<Geolocation.Geolocation>().WithParameter("filePath", "/data/geolocation/geolocation.json").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<Inbox>().WithParameter("sourcePath", new []{ "/data/source"}).AsImplementedInterfaces();
      builder.RegisterType<EventLogFactory>().AsImplementedInterfaces();
      builder.RegisterType<MediaImporter>().WithParameter("knownFileTypes", new []{"webp","jpeg","jpg","png","mp4","mts","avi","mkv"}).AsImplementedInterfaces();
      builder.RegisterType<GpxPositionsImporter>().AsImplementedInterfaces();
      builder.RegisterType<StopDetector>().AsImplementedInterfaces();
      builder.RegisterType<GoogleTakoutImporter>().AsImplementedInterfaces();
      builder.RegisterType<MediaImportHandlerFactory>().AsImplementedInterfaces();
      builder.RegisterType<PositionsImportHandlerFactory>().AsImplementedInterfaces();
      builder.RegisterAssemblyTypes(GetType().Assembly).Where(f => f.IsAssignableTo(typeof(IPositionsImportHandler)));
      builder.RegisterAssemblyTypes(GetType().Assembly).Where(f => f.IsAssignableTo(typeof(IMediaImportHandler)));
      //builder.RegisterType<EventLogUpdater>().AsImplementedInterfaces().WithParameter("basePath", "/data/eventsLog");
      //builder.RegisterType<CalculateDailyStats>().AsImplementedInterfaces().WithParameter("outputFileName", "/data/positions/distanceStats.csv");
      builder.RegisterType<MediaTypeDetector>().AsImplementedInterfaces()
        .WithParameter("moviesExtensions", new[] { "mp4", "mts", "mov" })
        .WithParameter("photosExtensions", new[] { "jpg", "jpeg", "webp" });
      builder.RegisterType<EventLogGenerator>().AsImplementedInterfaces().WithParameter("basePath", "/data/eventsLog/v2");
      builder.RegisterType<ActivityTypeDetector>().AsImplementedInterfaces();

      return builder.Build();
    }
  }
}