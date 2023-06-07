using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPreprocessor.Shared
{
  public class FilePath : ValueObject
  {
    private string _value;

    protected FilePath(string path)
    {
      _value = path;
    }

    public DirectoryPath Directory
    {
      get => System.IO.Path.GetDirectoryName(_value);
      set => _value = _value.Replace(System.IO.Path.GetDirectoryName(_value), value);
    }

    public string FileName => System.IO.Path.GetFileName(_value);
    public bool Exists => System.IO.File.Exists(_value);
    public string Extension => System.IO.Path.GetExtension(_value).ToLower().Replace(".", "");

    internal static FilePath Parse(string f)
    {
      return new FilePath(f);
    }

    public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(_value);

    protected override object[] GetValues()
    {
      return new[] { _value };
    }

    public void DeleteFile()
    {
      System.IO.File.Delete(_value);
    }

    public override string ToString()
    {
      return _value;
    }

    public static implicit operator string(FilePath path)
    {
      return path.ToString();
    }

    public static implicit operator FilePath(string path)
    {
      return new FilePath(path);
    }

    public bool Contains(string pathPart)
    {
      return _value.Contains(pathPart);
    }
  }
}
