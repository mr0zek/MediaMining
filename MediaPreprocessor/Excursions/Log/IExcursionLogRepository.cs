namespace MediaPreprocessor.Excursions.Log
{
  public interface IExcursionLogRepository
  {
    void SaveOrUpdate(ExcursionLog log);
  }
}