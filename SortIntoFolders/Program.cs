// See https://aka.ms/new-console-template for more information
using MediaPreprocessor.Media;
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

        FilePath[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Select(f => (FilePath)f).ToArray();

        files.AsParallel().ForAll(file =>
        {
          try
          {
            Media media = Media.FromFile(file);

            media.MoveTo(directory.AddDirectory(media.CreatedDate.Date.ToString("yyyy-MM-dd")).ToFilePath(file.FileName));

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
