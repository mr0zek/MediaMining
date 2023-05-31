using System;
using System.Collections.Generic;
using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Media;

namespace MediaMining.GpsUpdate
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
      builder.RegisterType<EventsImporter.EventsImporter>()        
        .AsImplementedInterfaces();
    }
  }
}
