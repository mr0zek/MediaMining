using System.IO;

namespace MediaPreprocessor.Excursions.Log
{
  internal class ExcursionLogRepository : IExcursionLogRepository
  {
    private readonly string _basePath;

    public ExcursionLogRepository(string basePath)
    {
      _basePath = basePath;
    }

    public void SaveOrUpdate(ExcursionLog log)
    {
      string directory = Path.Combine(_basePath, log.Excursion.GetUniqueName());
      string trackFilePath = Path.Combine(directory, $"{log.Excursion.GetUniqueName()}.geojson");
      log.WriteToFile(trackFilePath);
      string descriptionFilePath = Path.Combine(directory, $"{log.Excursion.GetUniqueName()}.md");
      log.WriteDescription(descriptionFilePath);
    }
  }
}