using System.Collections.Generic;

namespace MediaPreprocessor.Events
{
  public class Note
  {
    public string Name { get; set; }
    public List<string> Notes { get; set; } = new List<string>();

    public List<string> Links { get; set; } = new List<string>();
  }
}