using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Events
{
  public class Event
  {
    public string Name { get; set; }
    public Date DateFrom { get; set; }
    public Date DateTo { get; set; }
    
    public EventId Id
    {
      get
      {
        return new EventId(GetUniqueName());
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