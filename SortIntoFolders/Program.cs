// See https://aka.ms/new-console-template for more information
using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Events;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using NDesk.Options;

internal class Program
{
  static void Main(string[] args)
  {
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

        Console.WriteLine("Sorting files in directory : " + directory);

        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
        .Select(x => (FilePath)x)
         .Where(f => new MediaTypeDetector().IsKnownType(f));

        IContainer container = Bootstrap.BuildContainer(new string[0], (builder, options) =>
        {
          builder.RegisterType<MediaRepository>()
            .WithParameter("basePath", directory.ToString())
            .AsImplementedInterfaces();
          builder.RegisterType<PositionsRepository>()
            .WithParameter("basePath", directory.ToString())
            .AsImplementedInterfaces();
        });
        
        //files.AsParallel().ForAll(file =>
        files.ToList().ForEach(file =>
        {
          try
          {
            Media media = Media.FromFile(file);
            var eventFile = directory.ToFilePath("event.json");
            Event ev = null;
            if (eventFile.Exists)
            {
              ev = Event.FromFile(eventFile);
            }

            DirectoryPath dir = directory.AddDirectory(media.CreatedDate.Date.ToString("yyyy-MM-dd"));
            
            if (ev != null && media.CreatedDate >= ev.DateFrom && media.CreatedDate <= ev.DateTo)
            {
              var day = ev.GetDay(media.CreatedDate.Date);
              if (day != null)
              {
                dir = directory.AddDirectory(media.CreatedDate.Date.ToString("yyyy-MM-dd") + " - " + day.Name);
              }              
            }

            dir.Create();
            
            media.MoveTo(dir.ToFilePath(file.FileName));

            Console.WriteLine("Moved file to " + media.Path);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.ToString());
          }
        });
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
