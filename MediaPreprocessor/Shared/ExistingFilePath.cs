using System;
using System.IO;

namespace MediaPreprocessor.Shared
{
  public class ExistingFilePath : FilePath
  {
    public ExistingFilePath(string filePath) : base(filePath)
    {
      if (!File.Exists(filePath))
      {
        throw new ArgumentException($"File : {filePath} not exists");
      }
    }

    public static implicit operator string(ExistingFilePath path)
    {
      return path.ToString();
    }

    public static implicit operator ExistingFilePath(string path)
    {
      return new ExistingFilePath(path);
    }
  }
}