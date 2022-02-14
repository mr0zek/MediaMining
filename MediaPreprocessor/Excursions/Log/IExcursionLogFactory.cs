namespace MediaPreprocessor.Excursions.Log
{
  public interface IExcursionLogFactory
  {
    ExcursionLog Create(Excursion excursion);
  }
}