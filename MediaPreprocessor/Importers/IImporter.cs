using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Importers
{
  public interface IImporter
  {
    ISet<Date> Import(string filePath);
    bool CanImport(string filePath);
  }
}