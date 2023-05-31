using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Importers
{
  public interface IImporter
  {
    void Import(FilePath filePath);
    bool CanImport(FilePath filePath);
  }
}