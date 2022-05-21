using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPreprocessor.Shared
{
  public class FilePath : ValueObject
  {
    private readonly string _value;

    protected FilePath(string path)
    {
      _value = path;
    }

    public DirectoryPath Directory => System.IO.Path.GetDirectoryName(_value);
    public string FileName => System.IO.Path.GetFileName(_value);
    public bool Exists => System.IO.File.Exists(_value);
    public string Extension => System.IO.Path.GetExtension(_value);
    public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(_value);

    protected override object[] GetValues()
    {
      return new[] { _value };
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
  }
}
