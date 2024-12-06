using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Events;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using NDesk.Options;

internal class Program
{
  static void Main(string[] args)
  {
    bool show_help = false;
    DirectoryPath eventsDirectory = null;
    ExifData.ExifToolPath = Path.Combine(Environment.CurrentDirectory, "ExifTool", "exiftool.exe");

    var p = new OptionSet() {      
      { "p|positionsPath=", "Path to events folder", v => eventsDirectory = v },
      { "h|help=", "Show help", v => show_help = true }
    };

    try
    {
      List<string> extra;
      extra = p.Parse(args);

      if (extra.Count == 1)
      {
        DirectoryPath directory = extra[0];
        if (eventsDirectory == null)
        {
          eventsDirectory = directory.AddDirectory("events").ToString();
        }

        Console.WriteLine("Sorting files in directory : " + directory);

        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
        .Select(x => (FilePath)x)
         .Where(f => new MediaTypeDetector().IsKnownType(f)).ToList();

        IContainer container = Bootstrap.BuildContainer(new string[0], (builder, options) =>
        {
          builder.RegisterType<EventRepository>()
            .WithParameter("eventsPath", eventsDirectory.ToString())
            .AsImplementedInterfaces();
          builder.RegisterType<PositionsRepository>()
            .WithParameter("basePath", directory.ToString())
            .AsImplementedInterfaces();
        });

        int count = files.Count();
        int index = 0;

        var eventsRepository = container.Resolve<IEventRepository>();
      }
    }
    catch (Exception ex)
    {
    }
  }
}


