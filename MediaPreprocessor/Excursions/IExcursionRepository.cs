using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Excursions
{
  public interface IExcursionRepository
  {
    Excursion GetByDate(Date date);
    Excursion Get(ExcursionId excursionId);
  }
}