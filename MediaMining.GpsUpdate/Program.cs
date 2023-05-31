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

      var p = new string[]
      {
        $"source={args[0]}\\destination",
        $"positions={args[0]}\\positions",
        $"geolocation={args[0]}\\geolocation"
      };

      ExifData.ExifToolPath = $"{args[0]}\\exiftool\\exiftool.exe";

      new MyBootstrap().Run(p, 1, false);
    }
  }

  public class MyBootstrap : Bootstrap
  {
    protected override void RegisterExternals(ContainerBuilder builder, IDictionary<string, string> options)
    {
      builder.RegisterType<MediaGpsLocationUpdater>()
        .WithParameter("knownFileTypes", new[] { "webp", "jpeg", "jpg", "png", "mp4", "mts", "avi", "mkv" })
        .AsImplementedInterfaces();
    }
  }
}
