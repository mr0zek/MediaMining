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
      DateTime startDate = DateTime.MinValue;
      if(options.ContainsKey("startdate"))
      {
        startDate = DateTime.Parse(options["startdate"]);
      }

      builder.RegisterType<GpxPositionsImporter>().WithParameter("startDate", startDate).AsImplementedInterfaces();
      builder.RegisterType<GoogleTakoutImporter>().WithParameter("startDate", startDate).AsImplementedInterfaces();
      builder.RegisterType<GeojsonImporter>().WithParameter("startDate", startDate).AsImplementedInterfaces();
    }
  }
}
