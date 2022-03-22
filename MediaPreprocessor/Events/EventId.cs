using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Events
{
  public class EventId : ValueObject
  {
    private readonly string _id;

    public EventId(string id)
    {
      _id = id;
    }

    protected override object[] GetValues()
    {
      return new object[] {_id};
    }
  }
}
