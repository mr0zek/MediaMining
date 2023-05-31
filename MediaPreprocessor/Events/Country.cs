using System.Collections.Generic;

namespace MediaPreprocessor.Events
{
  public class Country
  {
    public string Name { get; set; }

    public List<string> Places { get; set; } = new List<string>();
  }
}