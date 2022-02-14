namespace MediaPreprocessor.Importers
{
  public interface IImporter
  {
    void Import(string filePath);
    bool CanImport(string filePath);
  }
}