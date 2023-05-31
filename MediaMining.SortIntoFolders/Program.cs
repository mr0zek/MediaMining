using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Handlers.MediaImportHandlers;
using MediaPreprocessor.Handlers.PostImportHandlers;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Media;
using System;
using System.Collections.Generic;

namespace MediaMining.SortIntoFolders
{
  internal class Program
  {
    static void Main(string[] args)
    {
      new MyBootstrap().Run(args, 1, false);
    }
  }

  public class MyBootstrap : Bootstrap
  {
    protected override void RegisterExternals(ContainerBuilder builder, IDictionary<string, string> options)
    {
      string destination = options.ContainsKey("destination") ? options["destination"] : "/data/destination";

      builder.RegisterType<SortFilesImporter>()
        .WithParameter("knownFileTypes", new[] { "webp", "jpeg", "jpg", "png", "mp4", "mts", "avi", "mkv" })
        .WithParameter("importToPath", destination)
        .AsImplementedInterfaces();
    }
  }
}
