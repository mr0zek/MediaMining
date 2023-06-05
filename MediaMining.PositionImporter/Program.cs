using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Importers;
using System;
using System.Collections.Generic;
using MediaPreprocessor.Media;

namespace MediaMining.PositionImporter
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
      builder.RegisterType<GpxPositionsImporter>().AsImplementedInterfaces();
      builder.RegisterType<GoogleTakoutImporter>().AsImplementedInterfaces();
      builder.RegisterType<GeojsonImporter>().AsImplementedInterfaces();
    }
  }
}
