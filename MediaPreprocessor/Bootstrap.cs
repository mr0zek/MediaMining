using System.Threading;
using Autofac;
using MediaPreprocessor.Excursions;
using MediaPreprocessor.Excursions.Log;
using MediaPreprocessor.Handlers;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor
{
  public class Bootstrap
  {
    //Media, Movie, Track, Action

    public void Run()
    {
      var container = BuildContainer();
      
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
      builder.RegisterType<ExcursionLogRepository>().WithParameter("basePath", "/data/excursionsLog").AsImplementedInterfaces();
      builder.RegisterType<ExcursionRepository>().WithParameter("excursionsPath", "/data/excursions").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<PositionsRepository>().WithParameter("path", "/data/positions").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<ExcursionLogFactory>().AsImplementedInterfaces();
      builder.RegisterType<Geolocation.Geolocation>().WithParameter("filePath", "/data/geolocation/geolocation.json").SingleInstance().AsImplementedInterfaces();
      builder.RegisterType<Inbox>().WithParameter("sourcePath", new []{ "/data/source"}).AsImplementedInterfaces();
      builder.RegisterType<ExcursionLogFactory>().AsImplementedInterfaces();
      builder.RegisterType<MediaImporter>().WithParameter("knownFileTypes", new []{"jpg","png","mp4","mts","avi","mkv"}).AsImplementedInterfaces();
      builder.RegisterType<GpxPositionsImporter>().AsImplementedInterfaces();
      builder.RegisterType<StopDetection>().AsImplementedInterfaces();
      builder.RegisterType<GoogleTakoutImporter>().AsImplementedInterfaces();
      builder.RegisterType<MediaImportHandlerFactory>().AsImplementedInterfaces();
      builder.RegisterType<PositionsImportHandlerFactory>().AsImplementedInterfaces();
      builder.RegisterAssemblyTypes(GetType().Assembly).Where(f => f.IsAssignableTo(typeof(IPositionsImportHandler)));
      builder.RegisterAssemblyTypes(GetType().Assembly).Where(f => f.IsAssignableTo(typeof(IMediaImportHandler)));

      return builder.Build();
    }
  }
}