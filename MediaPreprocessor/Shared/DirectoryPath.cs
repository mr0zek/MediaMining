using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPreprocessor.Shared
{
  public class DirectoryPath : ValueObject
  {
    private readonly string _value;

    protected DirectoryPath(string path)
    {
      _value = path;
    }

    public bool Exists => System.IO.Directory.Exists(_value);

    protected override object[] GetValues()
    {
      return new[] { _value };
    }

    public override string ToString()
    {
      return _value;
    }

    public static implicit operator string(DirectoryPath path)
    {
      return path.ToString();
    }

    public static implicit operator DirectoryPath(string path)
    {
      return new DirectoryPath(path);
    }

    public FilePath ToFilePath(string fileName)
    {
      return Path.Combine(_value, fileName);
    }

    public DirectoryPath AddDirectory(string directoryName)
    {
      return Path.Combine(_value, directoryName);
    }

    public void Create()
    {
      if (!Directory.Exists(_value))
      {
        Directory.CreateDirectory(_value);
      }
    }

    public static DirectoryPath Parse(string basePath)
    {
      return basePath;
    }
  }
}
