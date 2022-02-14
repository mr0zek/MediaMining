using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Importers
{
  public class Inbox : IInbox
  {
    private readonly string[] _sourcePath;
    private ILogger _log;

    public Inbox(string[] sourcePath, ILoggerFactory loggerFactory)
    {
      _sourcePath = sourcePath;
      _log = loggerFactory.CreateLogger<Inbox>();
    }

    public IEnumerable<string> GetFiles()
    {
      List<string> result = new List<string>();
      foreach (var s in _sourcePath)
      {
        result.AddRange(Directory.GetFiles(s, "*.*", SearchOption.AllDirectories));
      }

      return result;
    }

    public void Cleanup()
    {
      foreach (var s in _sourcePath)
      {
        RemoveEmptyFoldersFromSource(s);
      }
    }

    private bool RemoveEmptyFoldersFromSource(string sourcePath)
    {
      try
      {
        var directories = Directory.GetDirectories(sourcePath);
        foreach (var directory in directories)
        {
          if (RemoveEmptyFoldersFromSource(directory))
          {
            Directory.Delete(directory);
          }
        }

        var directories2 = Directory.GetDirectories(sourcePath);
        if (directories2.Length == 0)
        {
          if (Directory.GetFiles(sourcePath).Length == 0)
          {
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Error while removing empty folders");
      }

      return false;
    }
  }
}