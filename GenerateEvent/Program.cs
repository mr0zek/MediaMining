// See https://aka.ms/new-console-template for more information
using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Events;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Handlers.PostImportHandlers;
using MediaPreprocessor.MapGenerator;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;
using NDesk.Options;

internal class Program
{
  static void Main(string[] args)
  {
    bool show_help = false;
    string positionsPath;

    FilePath eventPath = null;
    var p = new OptionSet() {
      { "e|exifToolPath=", "Path to exif tool", v => ExifData.ExifToolPath = v },
      { "p|positionsPath=", "Path to positions", v => positionsPath = v },
      { "v|eventPath=", "Path to eventfile", v => eventPath = v },
      { "h|help=", "Show help", v => show_help = true }
    };

    try
    {
      List<string> extra;
      extra = p.Parse(args);

      if (extra.Count == 1)
      {
        DirectoryPath directory = extra[0];

        Console.WriteLine("Generating event for files in directory : " + directory);

        Event ev = Event.FromFile(eventPath);        

        var media = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)          
          .Where(f=>new MediaTypeDetector().IsKnownType(f))
          .Select(f => Media.FromFile(f)).ToList();

        IContainer container = Bootstrap.BuildContainer(new string[0], (builder, options) =>
        {
          builder.RegisterType<MediaRepository>()
            .WithParameter("basePath", directory)
            .AsImplementedInterfaces();
        });

        IPositionsRepository positionsRepository = container.Resolve<IPositionsRepository>();
        IStopDetector stopDetector = container.Resolve<IStopDetector>();
        IGeolocation geolocation = container.Resolve<IGeolocation>();

        for (Date day = ev.DateFrom; day <= ev.DateTo; day++)
        {
          var d = ev.GetDay(day);

          Track? positions = positionsRepository.GetFromDay(d.Date);
          IEnumerable<Stop>? stops = stopDetector.Detect(positions.Positions);

          foreach (var stop in stops)
          {
            var data = geolocation.GetReverseGeolocationData(stop.Position);
            d.Places.Add(new Place()
            {
              Time = stop.DateFrom,
              Duration = stop.Duration(),
              LocationName = data.LocationName
            });
          }
                    
          if(d.Description == null)
          {
            d.Description = $"Wyruszamy z {geolocation.GetReverseGeolocationData(positions.Positions.First()).LocationName} o godzinie {positions.DateFrom.ToString("HH:mm")}</br>";
            if (d.Places.Count > 0)
            {
              d.Description += $"Oddwiedzamy:</br>";
              foreach (var pl in d.Places)
              {
                d.Description += $"- {pl.LocationName}</br>";
              }
            }
            d.Description += $"Docieramy do {geolocation.GetReverseGeolocationData(positions.Positions.Last()).LocationName} o godzinie {positions.DateTo.ToString("HH:mm")}</br>";
          }                    
        }        

        foreach (var m in media)
        {
          if(m.GpsLocation == null)
          {
            m.GpsLocation = positionsRepository.Get(m.CreatedDate);            
          }
        }



        
        //IMapGenerator mg = container.Resolve<IMapGenerator>();

        //mg.Generate(ev, media, directory);

        var tl = container.Resolve<ITimelineGenerator>();

        tl.Generate(ev, media, directory);
      }
      else
      {
        show_help = true;
      }
    }
    catch (OptionException e)
    {
      Console.Write("greet: ");
      Console.WriteLine(e.Message);
      Console.WriteLine("Try `SortIntoFolders --help' for more information.");
      return;
    }

    if (show_help)
    {
      Console.WriteLine("Usage: SortIntoFolders [OPTIONS] directory");
      Console.WriteLine();
      Console.WriteLine("Options:");
      p.WriteOptionDescriptions(Console.Out);
      return;
    }
  }
}
