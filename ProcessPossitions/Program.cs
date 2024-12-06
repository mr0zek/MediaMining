// See https://aka.ms/new-console-template for more information
using Autofac;
using MediaMining.PositionImporter;
using MediaPreprocessor;
using MediaPreprocessor.Events;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Importers;
using MediaPreprocessor.MapGenerator;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using NDesk.Options;

bool show_help = false;

var p = new OptionSet() {
      { "e|exifToolPath=", "Path to exif tool", v => ExifData.ExifToolPath = v },      
      { "h|help=", "Show help", v => show_help = true }
    };

try
{
  List<string> extra;
  extra = p.Parse(args);

  if (extra.Count == 1)
  {
    DirectoryPath directory = extra[0];

    Console.WriteLine("Processing files from directory : " + directory);

    var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
    .Select(x => (FilePath)x)
     .Where(f => new MediaTypeDetector().IsKnownType(f));

    DateTime startDate = DateTime.MinValue;

    IContainer container = Bootstrap.BuildContainer(new string[0], (builder, options) =>
    {
      builder.RegisterType<PositionsRepository>()
        .WithParameter("basePath", directory.AddDirectory("positions").ToString())
        .AsImplementedInterfaces();
      builder.RegisterType<Geolocation>()
        .WithParameter("directoryPath", directory.ToString())
        .AsImplementedInterfaces();
      builder.RegisterType<Inbox>()
        .WithParameter("sourcePath", new string[] { directory.AddDirectory("positions").ToString() })
        .AsImplementedInterfaces();
      builder.RegisterType<GpxPositionsImporter>().WithParameter("startDate", startDate).AsImplementedInterfaces();
      builder.RegisterType<GoogleTakoutImporter>().WithParameter("startDate", startDate).AsImplementedInterfaces();
      builder.RegisterType<GeojsonImporter>().WithParameter("startDate", startDate).AsImplementedInterfaces();
    });

    Event ev = Event.FromFile(directory.ToFilePath("event.json"));

    IImporters importers = container.Resolve<IImporters>();

    importers.Import(true);

    var mapDirectory = directory.AddDirectory("map");
    mapDirectory.Create();

    IMapGenerator mapGenerator = container.Resolve<IMapGenerator>();
    mapGenerator.Generate(ev, new Media[] { }, mapDirectory);
  }
}
catch (OptionException e)
{
  Console.Write("greet: ");
  Console.WriteLine(e.Message);
  Console.WriteLine("Try ProcessPositions --help' for more information.");
  return;
}

if (show_help)
{
  Console.WriteLine("Usage: ProcessPositions [OPTIONS] directory");
  Console.WriteLine();
  Console.WriteLine("Options:");
  p.WriteOptionDescriptions(Console.Out);
  return;
}
  


