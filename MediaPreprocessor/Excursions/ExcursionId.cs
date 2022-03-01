using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Excursions
{
  public class ExcursionId : ValueObject
  {
    private readonly string _id;

    public ExcursionId(string id)
    {
      _id = id;
    }

    protected override object[] GetValues()
    {
      return new object[] {_id};
    }
  }
}
