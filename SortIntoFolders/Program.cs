// See https://aka.ms/new-console-template for more information
using Autofac;
using MediaPreprocessor;
using MediaPreprocessor.Events;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using NDesk.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SortIntoFolders
{
  class Program
  {    
    static void Main(string[] args)
    {
      bool show_help = false;
      DirectoryPath eventsDirectory = null;
      ExifData.ExifToolPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ExifTool", "exiftool.exe");

      var p = new OptionSet() {
      { "e|exifToolPath=", "Path to exif tool", v => ExifData.ExifToolPath = v },
      { "ev|eventsPath=", "Path to events folder", v => eventsDirectory = v },
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

          IContainer container = Bootstrap.BuildContainer(new string[0], (builder, options) =>
          {
            builder.RegisterType<MediaRepository>()
              .WithParameter("basePath", directory.ToString())
              .AsImplementedInterfaces();
            builder.RegisterType<EventRepository>()
              .WithParameter("eventsPath", eventsDirectory.ToString())
              .AsImplementedInterfaces();
            builder.RegisterType<PositionsRepository>()
              .WithParameter("basePath", directory.ToString())
              .AsImplementedInterfaces();
          });

          var eventsRepository = container.Resolve<IEventRepository>();

          DirectoryPath noExifDirectory = directory.AddDirectory("NoExifData");

          RemoveEmptyDirectories(directory);

          noExifDirectory.Create();

          var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
          .Select(x => (FilePath)x)
           .Where(f => new MediaTypeDetector().IsKnownType(f)).ToList();

          int count = files.Count();
          int index = 0;

          files.ToList().AsParallel().ForAll(file =>
          //files.ToList().ForEach(file =>
          {
            try
            {
              Media media = null;
              try
              {
                media = Media.FromFile(file);
              }
              catch
              {
                FilePath f = noExifDirectory.ToFilePath(file.FileName);
                File.Move(file, f, true);
                Console.WriteLine("No exif data, moving to: " + f);
                return;
              }

              Event ev = eventsRepository.GetByDate(media.CreatedDate);

              DirectoryPath dir = directory.AddDirectory(media.CreatedDate.Date.ToString("yyyy-MM-dd"));

              if (ev != null && media.CreatedDate >= ev.DateFrom && media.CreatedDate.Date <= ev.DateTo)
              {
                var day = ev.GetDay(media.CreatedDate.Date);
                if (day != null)
                {
                  if (day.Name != null)
                  {
                    dir = directory.AddDirectory($"{ev.DateFrom.ToString("yyyy-MM-dd")} - {ev.Name}").AddDirectory(media.CreatedDate.Date.ToString("yyyy-MM-dd") + " - " + day.Name);
                  }
                  else
                  {
                    dir = directory.AddDirectory($"{ev.DateFrom.ToString("yyyy-MM-dd")} - {ev.Name}").AddDirectory(media.CreatedDate.Date.ToString("yyyy-MM-dd"));
                  }
                }
                else
                {
                  dir = directory.AddDirectory($"{ev.DateFrom.ToString("yyyy-MM-dd")} - {ev.Name}");
                }
              }

              dir.Create();
              string prefix = "Day_" + media.CreatedDate.ToString("yyyy-MM-dd_HH-mm-ss");

              if (!file.FileName.StartsWith("Day_"))
              {
                file = file.Directory.ToFilePath(prefix + "_" + file.FileName);
              }
              else
              {
                file = file.Directory.ToFilePath(prefix + "_" + file.FileName[24..]);
              }

              var destinationFileName = dir.ToFilePath(file.FileName);

              if (media.Path != destinationFileName)
              {
                if (File.Exists(destinationFileName))
                {
                  File.Delete(destinationFileName);
                }
                media.MoveTo(destinationFileName);

                Console.WriteLine($"{index}/{count} - Moved file to " + media.Path);
              }
              else
              {
                Console.WriteLine($"{index}/{count} - File in right place, not moving (" + media.Path + ")");
              }

              Interlocked.Add(ref index, 1);
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.ToString());
            }
          });

          RemoveEmptyDirectories(directory);
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

    private static void RemoveEmptyDirectories(DirectoryPath directory)
    {
      var directories = Directory.GetDirectories(directory, "*.*", SearchOption.AllDirectories);
      foreach (var item in directories)
      {
        if (Directory.GetFiles(item, "*.*").Length == 0 && Directory.GetDirectories(item).Length == 0)
        {
          try
          {
            Directory.Delete(item, true);
            Console.WriteLine($"Removed: {item}");
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex);
          }
        }
      }
    }
  }
}