using System;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  public class MediaId : ValueObject
  {
    private readonly string _description;
    private readonly string _id;

    public MediaId(string description)
    {
      _description = description;
      _id = Guid.NewGuid().ToString();
    }

    public override string ToString()
    {
      return _id+", "+_description;
    }

    protected override object[] GetValues()
    {
      return new[] {_id};
    }

    public static MediaId NewId()
    {
      return new MediaId(Guid.NewGuid().ToString());
    }
  }
}