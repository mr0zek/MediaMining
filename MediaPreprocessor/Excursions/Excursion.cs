using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Excursions
{
  public class Excursion
  {
    public string Name { get; set; }
    public Date DateFrom { get; set; }
    public Date DateTo { get; set; }
    
    public ExcursionId Id
    {
      get
      {
        return new ExcursionId(GetUniqueName());
      }
    }

    public bool InEvent(Date date)
    {
      return date <= DateTo && date >= DateFrom;
    }

    public string GetUniqueName()
    {
      return $"{DateFrom} - {Name}";
    }
  }
}