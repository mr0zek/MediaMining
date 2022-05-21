using System;
using System.IO;

namespace MediaPreprocessor.Shared
{
  public class ExistingDirectoryPath : DirectoryPath
  {
    private ExistingDirectoryPath(string path) : base(path)
    {
      if (!Directory.Exists(path))
      {
        throw new ArgumentException($"Directory : {path} not exists");
      }
    }

    public static implicit operator string(ExistingDirectoryPath path)
    {
      return path.ToString();
    }

    public static implicit operator ExistingDirectoryPath(string path)
    {
      return new ExistingDirectoryPath(path);
    }
  }
}